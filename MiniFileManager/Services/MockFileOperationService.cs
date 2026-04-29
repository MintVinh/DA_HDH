using MiniFileManager.Models;

namespace MiniFileManager.Services;

public class MockFileOperationService : IFileOperationService
{
    public void CreateFolder(string parentPath, string folderName)
    {
    }

    public void DeleteItem(string path)
    {
    }

    public void RenameItem(string path, string newName)
    {
    }

    public FileSystemItem GetProperties(string path)
    {
        return new FileSystemItem
        {
            Name = Path.GetFileName(path),
            FullPath = path,
            IsDirectory = Directory.Exists(path),
            Size = 0,
            LastModified = DateTime.Now,
            CreatedDate = DateTime.Now,
            Extension = Path.GetExtension(path),
            IsHidden = false
        };
    }

    public void CopyItem(string sourcePath, string destinationPath)
    {
    }

    public void CutItem(string sourcePath, string destinationPath)
    {
    }

    public void PasteItem()
    {
    }

    public IEnumerable<string> SearchFiles(string rootPath, string pattern)
    {
        return Enumerable.Empty<string>();
    }
}
