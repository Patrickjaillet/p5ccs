using Microsoft.Win32;
using P5CCS.Core.Dialogs;

namespace P5CCS.App.Services;

public sealed class WpfDialogService : IDialogService
{
    public string? ShowOpenFileDialog(string filter, string title)
    {
        var dialog = new OpenFileDialog
        {
            Filter = filter,
            Title = title,
            CheckFileExists = true,
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? ShowSaveFileDialog(string filter, string title, string defaultFileName)
    {
        var dialog = new SaveFileDialog
        {
            Filter = filter,
            Title = title,
            FileName = defaultFileName,
            OverwritePrompt = true,
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? ShowFolderBrowserDialog(string title)
    {
        var dialog = new OpenFolderDialog
        {
            Title = title,
        };

        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }
}
