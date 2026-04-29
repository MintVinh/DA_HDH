using System.Drawing;

namespace MiniFileManager.Helpers;

public static class IconHelper
{
    private static readonly ImageList ImageList = new() { ImageSize = new Size(16, 16) };

    public const string KeyFolder = "folder";
    public const string KeyFolderOpen = "folder_open";
    public const string KeyFile = "file";
    public const string KeyDrive = "drive";

    public static ImageList GetImageList()
    {
        if (ImageList.Images.Count > 0)
        {
            return ImageList;
        }

        ImageList.Images.Add(KeyFolder, SystemIcons.Application.ToBitmap());
        ImageList.Images.Add(KeyFolderOpen, SystemIcons.Application.ToBitmap());
        ImageList.Images.Add(KeyFile, SystemIcons.Application.ToBitmap());
        ImageList.Images.Add(KeyDrive, SystemIcons.Application.ToBitmap());
        return ImageList;
    }
}
