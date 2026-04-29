using MiniFileManager.Models;

namespace MiniFileManager.Services;

public interface IFileOperationService
{
    void CreateFolder(string parentPath, string folderName);
    void DeleteItem(string path);
    void RenameItem(string path, string newName);
    FileSystemItem GetProperties(string path);
    void CopyItem(string sourcePath, string destinationPath);
    void CutItem(string sourcePath, string destinationPath);
    void PasteItem();
    IEnumerable<string> SearchFiles(string rootPath, string pattern);
}
