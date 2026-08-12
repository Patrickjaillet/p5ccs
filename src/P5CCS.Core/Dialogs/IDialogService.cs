namespace P5CCS.Core.Dialogs;

public interface IDialogService
{
    string? ShowOpenFileDialog(string filter, string title);

    string? ShowSaveFileDialog(string filter, string title, string defaultFileName);

    string? ShowFolderBrowserDialog(string title);
}
