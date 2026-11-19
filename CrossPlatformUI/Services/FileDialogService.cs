using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace CrossPlatformUI.Services;

public class FileDialogService(TopLevel? target) : IFileDialogService
{
    public async Task<IStorageFile?> OpenFileAsync()
    {
        var files = await target!.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Open File",
            AllowMultiple = false
        });

        return files.Count >= 1 ? files[0] : null;
    }
    
    public async Task<IStorageFolder?> OpenFolderAsync()
    {
        var files = await target!.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Select ROM Output Folder",
            AllowMultiple = false
        });

        return files.Count >= 1 ? files[0] : null;
    }

    public async Task<IStorageFile?> SaveFileAsync(string title = "Save File", string? suggestedName = null,
                                                   FilePickerFileType? fileType = null)
    {
        return await target!.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            Title = title,
            SuggestedFileName = suggestedName,
            DefaultExtension = fileType is null ? null : "yaml",
            FileTypeChoices = fileType is null ? null : [fileType],
            ShowOverwritePrompt = true
        });
    }
}