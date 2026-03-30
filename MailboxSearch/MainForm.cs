using System.Collections.Concurrent;
using System.Diagnostics;
using Serilog;

namespace MailboxSearch;

public partial class MainForm : Form
{
    private readonly EmailSearchService _searchService = new();
    private readonly FolderPreferenceStore _folderPreferenceStore = new();
    private readonly ConcurrentQueue<EmailSearchResult> _pendingResults = new();
    private readonly System.Windows.Forms.Timer _uiRefreshTimer = new();
    private CancellationTokenSource? _searchCancellationTokenSource;
    private volatile int _progressCurrentMessageNumber;
    private volatile int _progressTotalMessages;

    public MainForm()
    {
        InitializeComponent();
        folderBrowserDialog.RootFolder = Environment.SpecialFolder.Desktop;
        sortByComboBox.DataSource = Enum.GetValues<SearchSortCriterion>();
        sortByComboBox.SelectedItem = SearchSortCriterion.Date;
        _uiRefreshTimer.Interval = 100;
        _uiRefreshTimer.Tick += uiRefreshTimer_Tick;
    }

    private async void Form1_Load(object sender, EventArgs e)
    {
        try
        {
            directoryTextBox.Text = FolderPreferenceStore.LoadFolderPath();
            await SearchIfPossibleAsync();
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, exception.Message, "Search failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void browseButton_Click(object sender, EventArgs e)
    {
        if (Directory.Exists(directoryTextBox.Text))
        {
            folderBrowserDialog.SelectedPath = directoryTextBox.Text;
        }
    
        if (folderBrowserDialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        directoryTextBox.Text = folderBrowserDialog.SelectedPath;
        PersistSelectedDirectory();
    }

    private async void searchButton_Click(object sender, EventArgs e)
    {
        try
        {
            await SearchIfPossibleAsync();
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, exception.Message, "Search failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void cancelButton_Click(object sender, EventArgs e)
    {
        cancelButton.Enabled = false;
        _searchCancellationTokenSource?.Cancel();
    }

    private void resetButton_Click(object sender, EventArgs e)
    {
        queryTextBox.Clear();
        dateFromPicker.Checked = false;
        dateToPicker.Checked = false;
        sortByComboBox.SelectedItem = SearchSortCriterion.Date;
        resultsListView.Items.Clear();
        ClearPreview();
        statusLabel.Text = "Search criteria cleared.";
        queryTextBox.Focus();
    }

    private void resultsListView_DoubleClick(object sender, EventArgs e)
    {
        if (resultsListView.SelectedItems.Count == 0)
        {
            return;
        }

        if (resultsListView.SelectedItems[0].Tag is not EmailSearchResult result)
        {
            return;
        }

        if (!File.Exists(result.FilePath))
        {
            MessageBox.Show(this, "The selected EML file no longer exists.", "File not found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(result.FilePath)
            {
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Unable to open the file. {ex.Message}", "Open failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void queryTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
        {
            return;
        }

        e.SuppressKeyPress = true;
        await SearchIfPossibleAsync();
    }

    private void directoryTextBox_Leave(object sender, EventArgs e)
    {
        PersistSelectedDirectory();
    }

    private async Task SearchIfPossibleAsync()
    {
        string folderPath = directoryTextBox.Text.Trim();
        string query = queryTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(folderPath))
        {
            statusLabel.Text = "Select a folder with EML files.";
            resultsListView.Items.Clear();
            return;
        }

        if (!Directory.Exists(folderPath))
        {
            statusLabel.Text = "The selected folder does not exist.";
            resultsListView.Items.Clear();
            return;
        }

        if (string.IsNullOrWhiteSpace(query))
        {
            statusLabel.Text = "Enter search keywords or an exact phrase in double quotes.";
            resultsListView.Items.Clear();
            return;
        }

        SearchOptions searchOptions = CreateSearchOptions(query);
        if (searchOptions.DateFrom is { } dateFrom && searchOptions.DateTo is { } dateTo && dateFrom > dateTo)
        {
            statusLabel.Text = "Date from must be earlier than or equal to date to.";
            resultsListView.Items.Clear();
            ClearPreview();
            return;
        }

        PersistSelectedDirectory();
        _searchCancellationTokenSource?.Dispose();
        _searchCancellationTokenSource = new CancellationTokenSource();
        _progressCurrentMessageNumber = 0;
        _progressTotalMessages = 0;
        ClearPendingResultsQueue();

        ToggleSearchControls(isSearching: true);
        statusLabel.Text = "Searching 0 of 0...";
        resultsListView.Items.Clear();
        ClearPreview();
        _uiRefreshTimer.Start();

        try
        {
            IReadOnlyList<EmailSearchResult> results = await _searchService.SearchAsync(
                folderPath,
                searchOptions,
                UpdateSearchProgress,
                QueueSearchResult,
                _searchCancellationTokenSource.Token);
            _uiRefreshTimer.Stop();
            ClearPendingResultsQueue();
            PopulateResults(results);
            statusLabel.Text = $"Found {results.Count} matching message(s).";
        }
        catch (OperationCanceledException)
        {
            FlushPendingUiUpdates();
            statusLabel.Text = "Search cancelled";
            Log.Information(
                "Search cancelled by user for folder {FolderPath} and query {QueryText}.",
                folderPath,
                query);
        }
        catch (Exception ex)
        {
            FlushPendingUiUpdates();
            statusLabel.Text = "Search failed.";
            Log.Error(
                ex,
                "Search failed unexpectedly for folder {FolderPath} and query {QueryText}.",
                folderPath,
                query);
            MessageBox.Show(this, $"Search failed. {ex.Message}", "Search error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _uiRefreshTimer.Stop();
            _searchCancellationTokenSource?.Dispose();
            _searchCancellationTokenSource = null;
            ToggleSearchControls(isSearching: false);
        }
    }

    private void PopulateResults(IReadOnlyList<EmailSearchResult> results)
    {
        resultsListView.BeginUpdate();

        try
        {
            resultsListView.Items.Clear();

            foreach (EmailSearchResult result in results)
            {
                resultsListView.Items.Add(CreateResultItem(result));
            }

            if (resultsListView.Items.Count > 0)
            {
                resultsListView.Items[0].Selected = true;
            }
        }
        finally
        {
            resultsListView.EndUpdate();
        }
    }

    private void QueueSearchResult(EmailSearchResult result)
    {
        _pendingResults.Enqueue(result);
    }

    private void UpdateSearchProgress(SearchProgress searchProgress)
    {
        _progressCurrentMessageNumber = searchProgress.CurrentMessageNumber;
        _progressTotalMessages = searchProgress.TotalMessages;
    }

    private void uiRefreshTimer_Tick(object? sender, EventArgs e)
    {
        FlushPendingUiUpdates();
    }

    private void FlushPendingUiUpdates()
    {
        FlushPendingResults();

        if (_searchCancellationTokenSource is not null)
        {
            statusLabel.Text = $"Searching {_progressCurrentMessageNumber} of {_progressTotalMessages}...";
        }
    }

    private void FlushPendingResults()
    {
        if (_pendingResults.IsEmpty)
        {
            return;
        }

        resultsListView.BeginUpdate();

        try
        {
            int addedCount = 0;
            while (addedCount < 200 && _pendingResults.TryDequeue(out EmailSearchResult? result))
            {
                AddSearchResult(result);
                addedCount++;
            }
        }
        finally
        {
            resultsListView.EndUpdate();
        }
    }

    private void AddSearchResult(EmailSearchResult result)
    {
        ListViewItem item = CreateResultItem(result);
        resultsListView.Items.Add(item);

        if (resultsListView.Items.Count == 1)
        {
            item.Selected = true;
        }
    }

    private void resultsListView_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (resultsListView.SelectedItems.Count == 0 || resultsListView.SelectedItems[0].Tag is not EmailSearchResult result)
        {
            ClearPreview();
            return;
        }

        previewSubjectValueLabel.Text = result.Subject;
        previewFromValueLabel.Text = string.IsNullOrWhiteSpace(result.Sender) ? "-" : result.Sender;
        previewDateValueLabel.Text = string.IsNullOrWhiteSpace(result.DateDisplay) ? "-" : result.DateDisplay;
        previewBodyTextBox.Text = string.IsNullOrWhiteSpace(result.PreviewText)
            ? "No preview text is available for this message."
            : result.PreviewText;
    }

    private void ClearPreview()
    {
        previewSubjectValueLabel.Text = "-";
        previewFromValueLabel.Text = "-";
        previewDateValueLabel.Text = "-";
        previewBodyTextBox.Clear();
    }

    private SearchOptions CreateSearchOptions(string query)
    {
        return new SearchOptions
        {
            QueryText = query,
            DateFrom = GetSelectedDateFrom(),
            DateTo = GetSelectedDateTo(),
            SortCriterion = sortByComboBox.SelectedItem is SearchSortCriterion selectedSortCriterion
                ? selectedSortCriterion
                : SearchSortCriterion.Date
        };
    }

    private static DateTimeOffset CreateLocalDateBoundary(DateTime dateValue, bool endOfDay)
    {
        DateTime localDateTime = DateTime.SpecifyKind(
            endOfDay ? dateValue.Date.AddDays(1).AddTicks(-1) : dateValue.Date,
            DateTimeKind.Local);

        return new DateTimeOffset(localDateTime);
    }

    private DateTimeOffset? GetSelectedDateFrom()
    {
        return dateFromPicker.Checked ? CreateLocalDateBoundary(dateFromPicker.Value, endOfDay: false) : null;
    }

    private DateTimeOffset? GetSelectedDateTo()
    {
        return dateToPicker.Checked ? CreateLocalDateBoundary(dateToPicker.Value, endOfDay: true) : null;
    }

    private static ListViewItem CreateResultItem(EmailSearchResult result)
    {
        ListViewItem item = new ListViewItem(result.Subject);
        item.SubItems.Add(result.DateDisplay);
        item.SubItems.Add(result.Sender);
        item.SubItems.Add(result.FilePath);
        item.Tag = result;
        return item;
    }

    private void ClearPendingResultsQueue()
    {
        while (_pendingResults.TryDequeue(out _))
        {
        }
    }

    private void ToggleSearchControls(bool isSearching)
    {
        browseButton.Enabled = !isSearching;
        searchButton.Enabled = !isSearching;
        resetButton.Enabled = !isSearching;
        queryTextBox.Enabled = !isSearching;
        directoryTextBox.Enabled = !isSearching;
        dateFromPicker.Enabled = !isSearching;
        dateToPicker.Enabled = !isSearching;
        sortByComboBox.Enabled = !isSearching;
        cancelButton.Enabled = isSearching;
        cancelButton.Visible = isSearching;
        UseWaitCursor = false;
    }

    private void PersistSelectedDirectory()
    {
        string folderPath = directoryTextBox.Text.Trim();
        if (Directory.Exists(folderPath))
        {
            FolderPreferenceStore.SaveFolderPath(folderPath);
        }
    }
}
