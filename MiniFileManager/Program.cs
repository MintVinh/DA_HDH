using MiniFileManager.Forms;
using MiniFileManager.Services;

namespace MiniFileManager;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        IFileOperationService fileService = new BasicIOService();
        Application.Run(new MainForm(fileService));
    }
}
