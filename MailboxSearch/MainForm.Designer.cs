namespace MailboxSearch;

partial class MainForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        mainMenuStrip = new MenuStrip();
        fileToolStripMenuItem = new ToolStripMenuItem();
        cleanupToolStripMenuItem = new ToolStripMenuItem();
        exitToolStripMenuItem = new ToolStripMenuItem();
        mainLayoutPanel = new TableLayoutPanel();
        directoryLabel = new Label();
        directoryTextBox = new TextBox();
        browseButton = new Button();
        queryLabel = new Label();
        queryTextBox = new TextBox();
        buttonPanel = new FlowLayoutPanel();
        searchButton = new Button();
        resetButton = new Button();
        cancelButton = new Button();
        filterPanel = new FlowLayoutPanel();
        dateFromLabel = new Label();
        dateFromPicker = new DateTimePicker();
        dateToLabel = new Label();
        dateToPicker = new DateTimePicker();
        sortByLabel = new Label();
        sortByComboBox = new ComboBox();
        contentSplitContainer = new SplitContainer();
        resultsListView = new ListView();
        subjectColumnHeader = new ColumnHeader();
        dateColumnHeader = new ColumnHeader();
        fromColumnHeader = new ColumnHeader();
        pathColumnHeader = new ColumnHeader();
        previewLayoutPanel = new TableLayoutPanel();
        previewSubjectLabel = new Label();
        previewSubjectValueLabel = new Label();
        previewFromLabel = new Label();
        previewFromValueLabel = new Label();
        previewDateLabel = new Label();
        previewDateValueLabel = new Label();
        previewBodyTextBox = new TextBox();
        statusLabel = new Label();
        folderBrowserDialog = new FolderBrowserDialog();
        ((System.ComponentModel.ISupportInitialize)contentSplitContainer).BeginInit();
        contentSplitContainer.Panel1.SuspendLayout();
        contentSplitContainer.Panel2.SuspendLayout();
        contentSplitContainer.SuspendLayout();
        previewLayoutPanel.SuspendLayout();
        mainLayoutPanel.SuspendLayout();
        buttonPanel.SuspendLayout();
        mainMenuStrip.SuspendLayout();
        SuspendLayout();
        // 
        // mainMenuStrip
        // 
        mainMenuStrip.ImageScalingSize = new Size(20, 20);
        mainMenuStrip.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem });
        mainMenuStrip.Location = new Point(12, 12);
        mainMenuStrip.Name = "mainMenuStrip";
        mainMenuStrip.Size = new Size(1064, 28);
        mainMenuStrip.TabIndex = 1;
        mainMenuStrip.Text = "menuStrip1";
        // 
        // fileToolStripMenuItem
        // 
        fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { cleanupToolStripMenuItem, exitToolStripMenuItem });
        fileToolStripMenuItem.Name = "fileToolStripMenuItem";
        fileToolStripMenuItem.Size = new Size(46, 24);
        fileToolStripMenuItem.Text = "File";
        // 
        // cleanupToolStripMenuItem
        // 
        cleanupToolStripMenuItem.Name = "cleanupToolStripMenuItem";
        cleanupToolStripMenuItem.Size = new Size(147, 26);
        cleanupToolStripMenuItem.Text = "Cleanup";
        cleanupToolStripMenuItem.Click += cleanupToolStripMenuItem_Click;
        // 
        // exitToolStripMenuItem
        // 
        exitToolStripMenuItem.Name = "exitToolStripMenuItem";
        exitToolStripMenuItem.Size = new Size(147, 26);
        exitToolStripMenuItem.Text = "Exit";
        exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
        // 
        // mainLayoutPanel
        // 
        mainLayoutPanel.ColumnCount = 3;
        mainLayoutPanel.ColumnStyles.Add(new ColumnStyle());
        mainLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        mainLayoutPanel.ColumnStyles.Add(new ColumnStyle());
        mainLayoutPanel.Controls.Add(directoryLabel, 0, 0);
        mainLayoutPanel.Controls.Add(directoryTextBox, 1, 0);
        mainLayoutPanel.Controls.Add(browseButton, 2, 0);
        mainLayoutPanel.Controls.Add(queryLabel, 0, 1);
        mainLayoutPanel.Controls.Add(queryTextBox, 1, 1);
        mainLayoutPanel.Controls.Add(buttonPanel, 2, 1);
        mainLayoutPanel.Controls.Add(filterPanel, 0, 2);
        mainLayoutPanel.Controls.Add(contentSplitContainer, 0, 3);
        mainLayoutPanel.Controls.Add(statusLabel, 0, 4);
        mainLayoutPanel.Dock = DockStyle.Fill;
        mainLayoutPanel.Location = new Point(12, 40);
        mainLayoutPanel.Name = "mainLayoutPanel";
        mainLayoutPanel.RowCount = 5;
        mainLayoutPanel.RowStyles.Add(new RowStyle());
        mainLayoutPanel.RowStyles.Add(new RowStyle());
        mainLayoutPanel.RowStyles.Add(new RowStyle());
        mainLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        mainLayoutPanel.RowStyles.Add(new RowStyle());
        mainLayoutPanel.Size = new Size(1064, 609);
        mainLayoutPanel.TabIndex = 0;
        mainLayoutPanel.SetColumnSpan(filterPanel, 3);
        mainLayoutPanel.SetColumnSpan(contentSplitContainer, 3);
        mainLayoutPanel.SetColumnSpan(statusLabel, 3);
        // 
        // directoryLabel
        // 
        directoryLabel.Anchor = AnchorStyles.Left;
        directoryLabel.AutoSize = true;
        directoryLabel.Location = new Point(3, 8);
        directoryLabel.Name = "directoryLabel";
        directoryLabel.Size = new Size(95, 20);
        directoryLabel.TabIndex = 0;
        directoryLabel.Text = "EML Folder:";
        // 
        // directoryTextBox
        // 
        directoryTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        directoryTextBox.Location = new Point(104, 3);
        directoryTextBox.Name = "directoryTextBox";
        directoryTextBox.Size = new Size(824, 27);
        directoryTextBox.TabIndex = 1;
        directoryTextBox.Leave += directoryTextBox_Leave;
        // 
        // browseButton
        // 
        browseButton.AutoSize = true;
        browseButton.Location = new Point(934, 3);
        browseButton.Name = "browseButton";
        browseButton.Size = new Size(127, 30);
        browseButton.TabIndex = 2;
        browseButton.Text = "Browse...";
        browseButton.UseVisualStyleBackColor = true;
        browseButton.Click += browseButton_Click;
        // 
        // queryLabel
        // 
        queryLabel.Anchor = AnchorStyles.Left;
        queryLabel.AutoSize = true;
        queryLabel.Location = new Point(3, 45);
        queryLabel.Name = "queryLabel";
        queryLabel.Size = new Size(49, 20);
        queryLabel.TabIndex = 3;
        queryLabel.Text = "Query:";
        // 
        // queryTextBox
        // 
        queryTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        queryTextBox.Location = new Point(104, 40);
        queryTextBox.Name = "queryTextBox";
        queryTextBox.PlaceholderText = "Enter keywords or an exact phrase in double quotes";
        queryTextBox.Size = new Size(824, 27);
        queryTextBox.TabIndex = 4;
        queryTextBox.KeyDown += queryTextBox_KeyDown;
        // 
        // buttonPanel
        // 
        buttonPanel.AutoSize = true;
        buttonPanel.Controls.Add(searchButton);
        buttonPanel.Controls.Add(resetButton);
        buttonPanel.Controls.Add(cancelButton);
        buttonPanel.FlowDirection = FlowDirection.LeftToRight;
        buttonPanel.Location = new Point(934, 36);
        buttonPanel.Margin = new Padding(3, 0, 3, 0);
        buttonPanel.Name = "buttonPanel";
        buttonPanel.Size = new Size(206, 35);
        buttonPanel.TabIndex = 5;
        buttonPanel.WrapContents = false;
        // 
        // searchButton
        // 
        searchButton.AutoSize = true;
        searchButton.Location = new Point(3, 3);
        searchButton.Name = "searchButton";
        searchButton.Size = new Size(75, 29);
        searchButton.TabIndex = 0;
        searchButton.Text = "Search";
        searchButton.UseVisualStyleBackColor = true;
        searchButton.Click += searchButton_Click;
        // 
        // resetButton
        // 
        resetButton.AutoSize = true;
        resetButton.Location = new Point(84, 3);
        resetButton.Name = "resetButton";
        resetButton.Size = new Size(40, 29);
        resetButton.TabIndex = 1;
        resetButton.Text = "Reset";
        resetButton.UseVisualStyleBackColor = true;
        resetButton.Click += resetButton_Click;
        // 
        // cancelButton
        // 
        cancelButton.AutoSize = true;
        cancelButton.Enabled = false;
        cancelButton.Location = new Point(130, 3);
        cancelButton.Name = "cancelButton";
        cancelButton.Size = new Size(73, 29);
        cancelButton.TabIndex = 2;
        cancelButton.Text = "Cancel";
        cancelButton.UseVisualStyleBackColor = true;
        cancelButton.Visible = false;
        cancelButton.Click += cancelButton_Click;
        // 
        // filterPanel
        // 
        filterPanel.AutoSize = true;
        filterPanel.Controls.Add(dateFromLabel);
        filterPanel.Controls.Add(dateFromPicker);
        filterPanel.Controls.Add(dateToLabel);
        filterPanel.Controls.Add(dateToPicker);
        filterPanel.Controls.Add(sortByLabel);
        filterPanel.Controls.Add(sortByComboBox);
        filterPanel.Dock = DockStyle.Fill;
        filterPanel.Location = new Point(3, 74);
        filterPanel.Margin = new Padding(3, 3, 3, 6);
        filterPanel.Name = "filterPanel";
        filterPanel.Size = new Size(1058, 34);
        filterPanel.TabIndex = 6;
        filterPanel.WrapContents = false;
        // 
        // dateFromLabel
        // 
        dateFromLabel.Anchor = AnchorStyles.Left;
        dateFromLabel.AutoSize = true;
        dateFromLabel.Location = new Point(3, 7);
        dateFromLabel.Margin = new Padding(3, 7, 6, 0);
        dateFromLabel.Name = "dateFromLabel";
        dateFromLabel.Size = new Size(78, 20);
        dateFromLabel.TabIndex = 0;
        dateFromLabel.Text = "Date from:";
        // 
        // dateFromPicker
        // 
        dateFromPicker.Checked = false;
        dateFromPicker.Format = DateTimePickerFormat.Short;
        dateFromPicker.Location = new Point(90, 3);
        dateFromPicker.Name = "dateFromPicker";
        dateFromPicker.ShowCheckBox = true;
        dateFromPicker.Size = new Size(148, 27);
        dateFromPicker.TabIndex = 1;
        // 
        // dateToLabel
        // 
        dateToLabel.Anchor = AnchorStyles.Left;
        dateToLabel.AutoSize = true;
        dateToLabel.Location = new Point(250, 7);
        dateToLabel.Margin = new Padding(9, 7, 6, 0);
        dateToLabel.Name = "dateToLabel";
        dateToLabel.Size = new Size(61, 20);
        dateToLabel.TabIndex = 2;
        dateToLabel.Text = "Date to:";
        // 
        // dateToPicker
        // 
        dateToPicker.Checked = false;
        dateToPicker.Format = DateTimePickerFormat.Short;
        dateToPicker.Location = new Point(320, 3);
        dateToPicker.Name = "dateToPicker";
        dateToPicker.ShowCheckBox = true;
        dateToPicker.Size = new Size(148, 27);
        dateToPicker.TabIndex = 3;
        // 
        // sortByLabel
        // 
        sortByLabel.Anchor = AnchorStyles.Left;
        sortByLabel.AutoSize = true;
        sortByLabel.Location = new Point(480, 7);
        sortByLabel.Margin = new Padding(9, 7, 6, 0);
        sortByLabel.Name = "sortByLabel";
        sortByLabel.Size = new Size(58, 20);
        sortByLabel.TabIndex = 4;
        sortByLabel.Text = "Sort by:";
        // 
        // sortByComboBox
        // 
        sortByComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        sortByComboBox.FormattingEnabled = true;
        sortByComboBox.Location = new Point(547, 3);
        sortByComboBox.Name = "sortByComboBox";
        sortByComboBox.Size = new Size(140, 28);
        sortByComboBox.TabIndex = 5;
        // 
        // 
        // contentSplitContainer
        // 
        contentSplitContainer.Dock = DockStyle.Fill;
        contentSplitContainer.Location = new Point(3, 114);
        contentSplitContainer.Name = "contentSplitContainer";
        contentSplitContainer.Orientation = Orientation.Horizontal;
        // 
        // contentSplitContainer.Panel1
        // 
        contentSplitContainer.Panel1.Controls.Add(resultsListView);
        // 
        // contentSplitContainer.Panel2
        // 
        contentSplitContainer.Panel2.Controls.Add(previewLayoutPanel);
        contentSplitContainer.Size = new Size(1058, 530);
        contentSplitContainer.SplitterDistance = 302;
        contentSplitContainer.TabIndex = 6;
        // 
        // resultsListView
        // 
        resultsListView.Columns.AddRange(new ColumnHeader[] { subjectColumnHeader, dateColumnHeader, fromColumnHeader, pathColumnHeader });
        resultsListView.Dock = DockStyle.Fill;
        resultsListView.FullRowSelect = true;
        resultsListView.GridLines = true;
        resultsListView.HideSelection = false;
        resultsListView.Location = new Point(0, 0);
        resultsListView.MultiSelect = false;
        resultsListView.Name = "resultsListView";
        resultsListView.Size = new Size(1058, 302);
        resultsListView.TabIndex = 0;
        resultsListView.UseCompatibleStateImageBehavior = false;
        resultsListView.View = View.Details;
        resultsListView.DoubleClick += resultsListView_DoubleClick;
        resultsListView.SelectedIndexChanged += resultsListView_SelectedIndexChanged;
        // 
        // subjectColumnHeader
        // 
        subjectColumnHeader.Text = "Title";
        subjectColumnHeader.Width = 360;
        // 
        // dateColumnHeader
        // 
        dateColumnHeader.Text = "Date";
        dateColumnHeader.Width = 160;
        // 
        // fromColumnHeader
        // 
        fromColumnHeader.Text = "From";
        fromColumnHeader.Width = 240;
        // 
        // pathColumnHeader
        // 
        pathColumnHeader.Text = "File Path";
        pathColumnHeader.Width = 500;
        // 
        // previewLayoutPanel
        // 
        previewLayoutPanel.ColumnCount = 2;
        previewLayoutPanel.ColumnStyles.Add(new ColumnStyle());
        previewLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        previewLayoutPanel.Controls.Add(previewSubjectLabel, 0, 0);
        previewLayoutPanel.Controls.Add(previewSubjectValueLabel, 1, 0);
        previewLayoutPanel.Controls.Add(previewFromLabel, 0, 1);
        previewLayoutPanel.Controls.Add(previewFromValueLabel, 1, 1);
        previewLayoutPanel.Controls.Add(previewDateLabel, 0, 2);
        previewLayoutPanel.Controls.Add(previewDateValueLabel, 1, 2);
        previewLayoutPanel.Controls.Add(previewBodyTextBox, 0, 3);
        previewLayoutPanel.Dock = DockStyle.Fill;
        previewLayoutPanel.Location = new Point(0, 0);
        previewLayoutPanel.Name = "previewLayoutPanel";
        previewLayoutPanel.RowCount = 4;
        previewLayoutPanel.RowStyles.Add(new RowStyle());
        previewLayoutPanel.RowStyles.Add(new RowStyle());
        previewLayoutPanel.RowStyles.Add(new RowStyle());
        previewLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        previewLayoutPanel.Size = new Size(1058, 224);
        previewLayoutPanel.TabIndex = 0;
        previewLayoutPanel.SetColumnSpan(previewBodyTextBox, 2);
        // 
        // previewSubjectLabel
        // 
        previewSubjectLabel.Anchor = AnchorStyles.Left;
        previewSubjectLabel.AutoSize = true;
        previewSubjectLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
        previewSubjectLabel.Location = new Point(3, 0);
        previewSubjectLabel.Name = "previewSubjectLabel";
        previewSubjectLabel.Size = new Size(58, 20);
        previewSubjectLabel.TabIndex = 0;
        previewSubjectLabel.Text = "Subject:";
        // 
        // previewSubjectValueLabel
        // 
        previewSubjectValueLabel.AutoEllipsis = true;
        previewSubjectValueLabel.AutoSize = true;
        previewSubjectValueLabel.Dock = DockStyle.Fill;
        previewSubjectValueLabel.Location = new Point(67, 0);
        previewSubjectValueLabel.Name = "previewSubjectValueLabel";
        previewSubjectValueLabel.Size = new Size(988, 20);
        previewSubjectValueLabel.TabIndex = 1;
        previewSubjectValueLabel.Text = "-";
        // 
        // previewFromLabel
        // 
        previewFromLabel.Anchor = AnchorStyles.Left;
        previewFromLabel.AutoSize = true;
        previewFromLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
        previewFromLabel.Location = new Point(3, 20);
        previewFromLabel.Name = "previewFromLabel";
        previewFromLabel.Size = new Size(47, 20);
        previewFromLabel.TabIndex = 2;
        previewFromLabel.Text = "From:";
        // 
        // previewFromValueLabel
        // 
        previewFromValueLabel.AutoEllipsis = true;
        previewFromValueLabel.AutoSize = true;
        previewFromValueLabel.Dock = DockStyle.Fill;
        previewFromValueLabel.Location = new Point(67, 20);
        previewFromValueLabel.Name = "previewFromValueLabel";
        previewFromValueLabel.Size = new Size(988, 20);
        previewFromValueLabel.TabIndex = 3;
        previewFromValueLabel.Text = "-";
        // 
        // previewDateLabel
        // 
        previewDateLabel.Anchor = AnchorStyles.Left;
        previewDateLabel.AutoSize = true;
        previewDateLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
        previewDateLabel.Location = new Point(3, 40);
        previewDateLabel.Name = "previewDateLabel";
        previewDateLabel.Size = new Size(43, 20);
        previewDateLabel.TabIndex = 4;
        previewDateLabel.Text = "Date:";
        // 
        // previewDateValueLabel
        // 
        previewDateValueLabel.AutoEllipsis = true;
        previewDateValueLabel.AutoSize = true;
        previewDateValueLabel.Dock = DockStyle.Fill;
        previewDateValueLabel.Location = new Point(67, 40);
        previewDateValueLabel.Name = "previewDateValueLabel";
        previewDateValueLabel.Size = new Size(988, 20);
        previewDateValueLabel.TabIndex = 5;
        previewDateValueLabel.Text = "-";
        // 
        // previewBodyTextBox
        // 
        previewBodyTextBox.Dock = DockStyle.Fill;
        previewBodyTextBox.Location = new Point(3, 63);
        previewBodyTextBox.Multiline = true;
        previewBodyTextBox.Name = "previewBodyTextBox";
        previewBodyTextBox.ReadOnly = true;
        previewBodyTextBox.ScrollBars = ScrollBars.Both;
        previewBodyTextBox.Size = new Size(1052, 158);
        previewBodyTextBox.TabIndex = 6;
        previewBodyTextBox.WordWrap = false;
        // 
        // statusLabel
        // 
        statusLabel.AutoEllipsis = true;
        statusLabel.AutoSize = true;
        statusLabel.Dock = DockStyle.Fill;
        statusLabel.Location = new Point(3, 579);
        statusLabel.Name = "statusLabel";
        statusLabel.Padding = new Padding(0, 8, 0, 0);
        statusLabel.Size = new Size(1058, 30);
        statusLabel.TabIndex = 7;
        statusLabel.Text = "Select a folder and enter a search query.";
        // 
        // Form1
        // 
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1088, 661);
        Controls.Add(mainLayoutPanel);
        Controls.Add(mainMenuStrip);
        MainMenuStrip = mainMenuStrip;
        MinimumSize = new Size(900, 500);
        Name = "MainForm";
        Padding = new Padding(12);
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Mailbox Search";
        Load += Form1_Load;
        contentSplitContainer.Panel1.ResumeLayout(false);
        contentSplitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)contentSplitContainer).EndInit();
        contentSplitContainer.ResumeLayout(false);
        previewLayoutPanel.ResumeLayout(false);
        previewLayoutPanel.PerformLayout();
        mainLayoutPanel.ResumeLayout(false);
        mainLayoutPanel.PerformLayout();
        buttonPanel.ResumeLayout(false);
        buttonPanel.PerformLayout();
        mainMenuStrip.ResumeLayout(false);
        mainMenuStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private MenuStrip mainMenuStrip;
    private ToolStripMenuItem fileToolStripMenuItem;
    private ToolStripMenuItem cleanupToolStripMenuItem;
    private ToolStripMenuItem exitToolStripMenuItem;
    private TableLayoutPanel mainLayoutPanel;
    private Label directoryLabel;
    private TextBox directoryTextBox;
    private Button browseButton;
    private Label queryLabel;
    private TextBox queryTextBox;
    private FlowLayoutPanel buttonPanel;
    private Button searchButton;
    private Button resetButton;
    private Button cancelButton;
    private FlowLayoutPanel filterPanel;
    private Label dateFromLabel;
    private DateTimePicker dateFromPicker;
    private Label dateToLabel;
    private DateTimePicker dateToPicker;
    private Label sortByLabel;
    private ComboBox sortByComboBox;
    private SplitContainer contentSplitContainer;
    private ListView resultsListView;
    private ColumnHeader subjectColumnHeader;
    private ColumnHeader dateColumnHeader;
    private ColumnHeader fromColumnHeader;
    private ColumnHeader pathColumnHeader;
    private TableLayoutPanel previewLayoutPanel;
    private Label previewSubjectLabel;
    private Label previewSubjectValueLabel;
    private Label previewFromLabel;
    private Label previewFromValueLabel;
    private Label previewDateLabel;
    private Label previewDateValueLabel;
    private TextBox previewBodyTextBox;
    private Label statusLabel;
    private FolderBrowserDialog folderBrowserDialog;
}
