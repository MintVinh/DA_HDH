using MiniFileManager.Models;

namespace MiniFileManager.Services;

public class BasicIOService : IFileOperationService
{
    public void CreateFolder(string parentPath, string folderName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(parentPath) || string.IsNullOrWhiteSpace(folderName))
            {
                return;
            }

            var fullPath = Path.Combine(parentPath, folderName);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }
        catch
        {
        }
    }

    public void DeleteItem(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
                return;
            }

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
        }
    }

    public void RenameItem(string path, string newName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(newName))
            {
                return;
            }

            var parentPath = Path.GetDirectoryName(path);
            if (string.IsNullOrWhiteSpace(parentPath))
            {
                return;
            }

            var destinationPath = Path.Combine(parentPath, newName);
            if (Directory.Exists(path))
            {
                Directory.Move(path, destinationPath);
                return;
            }

            if (File.Exists(path))
            {
                File.Move(path, destinationPath);
            }
        }
        catch
        {
        }
    }

    public FileSystemItem GetProperties(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                var directoryInfo = new DirectoryInfo(path);
                return new FileSystemItem
                {
                    Name = directoryInfo.Name,
                    FullPath = directoryInfo.FullName,
                    IsDirectory = true,
                    Size = 0,
                    LastModified = directoryInfo.LastWriteTime,
                    CreatedDate = directoryInfo.CreationTime,
                    Extension = string.Empty,
                    IsHidden = directoryInfo.Attributes.HasFlag(FileAttributes.Hidden)
                };
            }

            if (File.Exists(path))
            {
                var fileInfo = new FileInfo(path);
                return new FileSystemItem
                {
                    Name = fileInfo.Name,
                    FullPath = fileInfo.FullName,
                    IsDirectory = false,
                    Size = fileInfo.Length,
                    LastModified = fileInfo.LastWriteTime,
                    CreatedDate = fileInfo.CreationTime,
                    Extension = fileInfo.Extension,
                    IsHidden = fileInfo.Attributes.HasFlag(FileAttributes.Hidden)
                };
            }
        }
        catch
        {
        }

        return new FileSystemItem();
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
