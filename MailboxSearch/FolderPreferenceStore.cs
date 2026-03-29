using Microsoft.Win32;

namespace MailboxSearch;

public sealed class FolderPreferenceStore
{
    private const string RegistryPath = @"Software\MailboxSearch";
    private const string FolderPathValueName = "SelectedFolderPath";

    public static string LoadFolderPath()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegistryPath, writable: false);
        string? value = key?.GetValue(FolderPathValueName) as string;
        return value ?? string.Empty;
    }

    public static void SaveFolderPath(string folderPath)
    {
        using RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath);
        key.SetValue(FolderPathValueName, folderPath, RegistryValueKind.String);
    }
}