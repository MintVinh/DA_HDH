using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics; // Thư viện để Debug.WriteLine ghi log ngầm

namespace MiniFileManagerCore
{
    public class AdvancedFileOps
    {
        // ====================================
        // PHẦN 1: TÌM KIẾM FILE (DÙNG ĐỆ QUY)
        // ====================================
        public List<string> SearchFiles(string rootPath, string keyword)
        {
            List<string> foundFiles = new List<string>();

            try
            {
                // 1. Tìm các file có tên chứa từ khóa trong folder hiện tại
                string[] files = Directory.GetFiles(rootPath, $"*{keyword}*");
                foundFiles.AddRange(files);

                // 2. Lấy danh sách các folder con
                string[] subDirs = Directory.GetDirectories(rootPath);

                // 3. Vào từng folder con để tìm tiếp
                foreach (string dir in subDirs)
                {
                    foundFiles.AddRange(SearchFiles(dir, keyword));
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Ứng dụng sẽ âm thầm bỏ qua folder hệ thống mà ko spam lỗi lên màn hình UI.
                // Chỉ dev bật chế độ Debug mới thấy dòng chữ này.
                Debug.WriteLine($"[Bỏ qua] Không có quyền vào: {rootPath}");
            }
            catch (Exception ex)
            {
                // Các lỗi lặt vặt khác cũng ghi log ngầm, ko làm phiền người dùng
                Debug.WriteLine($"[Lỗi Tìm Kiếm] {ex.Message}");
            }

            return foundFiles;
        }

        // ========================================
        // PHẦN 2: COPY FOLDER & FILE (BẤT ĐỒNG BỘ)
        // ========================================
        public async Task CopyDirectoryAsync(string sourceDir, string destDir)
        {
            // Nếu có lỗi, lỗi sẽ ném (throw) thẳng ra ngoài.
            // UI cần dùng try-catch ở nút bấm để hiện bảng báo lỗi.

            // 1. Tạo folder đích nếu chưa tồn tại
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            // 2. Lấy tất cả file ở folder nguồn rồi copy sang đích
            string[] files = Directory.GetFiles(sourceDir);
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(destDir, fileName);

                // Gọi hàm copy file ngầm
                await CopyFileAsync(file, destFile);
            }

            // 3. Đệ quy: Tự động copy tiếp các folder con bên trong
            string[] subDirs = Directory.GetDirectories(sourceDir);
            foreach (string subDir in subDirs)
            {
                string dirName = Path.GetFileName(subDir);
                string destSubDir = Path.Combine(destDir, dirName);

                await CopyDirectoryAsync(subDir, destSubDir);
            }
        }

        // Hàm hỗ trợ copy từng file (Chia nhỏ luồng dữ liệu)
        private async Task CopyFileAsync(string sourceFile, string destFile)
        {
            // Dùng FileStream để cắt file thành từng phần nhỏ (buffer) - 4KB
            // Lệnh 'using' tự động dọn dẹp RAM ngay sau khi copy xong
            using (FileStream sourceStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            using (FileStream destStream = new FileStream(destFile, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, true))
            {
                await sourceStream.CopyToAsync(destStream);
            }
        }
    }
}