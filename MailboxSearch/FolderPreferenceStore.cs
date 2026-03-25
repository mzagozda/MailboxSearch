using Microsoft.Win32;

namespace MailboxSearch;

public sealed class FolderPreferenceStore
{
    private const string RegistryPath = @"Software\MailboxSearch";
    private const string FolderPathValueName = "SelectedFolderPath";

    public string LoadFolderPath()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, writable: false);
        var value = key?.GetValue(FolderPathValueName) as string;
        return value ?? string.Empty;
    }

    public void SaveFolderPath(string folderPath)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RegistryPath);
        key.SetValue(FolderPathValueName, folderPath, RegistryValueKind.String);
    }
}