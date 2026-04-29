using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using MiniFileManager.Forms;
using MiniFileManager.Services;
using MiniFileManagerCore;

namespace MiniFileManager;

internal static class Program
{
    [STAThread]
    // Đổi void thành async Task để chạy hàm copy ngầm
    private static async Task Main()
    {
        ApplicationConfiguration.Initialize();

        // ================================
        // PHẦN CODE NGƯỜI SỐ 3 - TUẤN TRẦN
        // ================================
        AdvancedFileOps ops = new AdvancedFileOps();

        // 1. TÌM KIẾM
        // Nhớ đổi C:\Users
        var results = ops.SearchFiles(@"E:\File VIA", "json");
        //                    (@"Địa chỉ folder tìm", "từ khóa cần tìm");

        string ketQuaTim = $"Đã tìm thấy {results.Count} file!\n\nVí dụ file đầu tiên:\n" +
                           (results.Count > 0 ? results[0] : "Không tìm thấy gì");

        // Hiện box popup thông báo
        MessageBox.Show(ketQuaTim, "Kết quả Tìm Kiếm");


        // 2. COPY BẤT ĐỒNG BỘ
        // Nhớ đổi folder nguồn và folder đích nếu cần
        string thuMucNguon = @"E:\File VIA";
        string thuMucDich = @"E:\HDH";

        MessageBox.Show($"Bấm OK để bắt đầu copy từ:\n{thuMucNguon}\nSang:\n{thuMucDich}", "Chuẩn bị Copy");

        // Gọi hàm copy chạy ngầm 
        await ops.CopyDirectoryAsync(thuMucNguon, thuMucDich);

        MessageBox.Show("Copy hoàn tất!", "Copy Xong");


         IFileOperationService fileService = new BasicIOService();
         Application.Run(new MainForm(fileService));
    }
}