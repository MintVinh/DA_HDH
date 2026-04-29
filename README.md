# Mini File Manager

Ứng dụng quản lý file đơn giản bằng WinForms (.NET 6), hỗ trợ duyệt thư mục, mở file và thao tác file cơ bản/nâng cao theo mô hình phân tách service.

## Công nghệ

- C# WinForms (.NET 6)
- `System.IO`

## Cấu trúc project

```text
MiniFileManager/
├── Forms/
├── Services/
├── Models/
├── Helpers/
└── Program.cs
```

## Phân công

- Người 1: UI + Navigation (`MainForm`, `NavigationService`)
- Người 2: Basic I/O (`Create`, `Delete`, `Rename`, `GetProperties`)
- Người 3: Advanced I/O (`Copy`, `Cut`, `Paste`, `Search`)
- Người 4: OS/Core + logging + integration

## Nguyên tắc quan trọng

- **UI KHÔNG gọi trực tiếp `File`/`Directory`, phải đi qua `IFileOperationService`.**
- **Không sửa interface nếu chưa thống nhất cả nhóm.**
- **Không đổi tên các property trong `FileSystemItem`.**
- **Mỗi người chỉ code trong phần được phân công.**

## Cách chạy project

- Mở solution bằng Visual Studio 2022
- Đảm bảo máy đã cài .NET 6 SDK/Runtime
- Build solution và Run project `MiniFileManager`

## Quy trình làm việc

- Mỗi người làm trên branch riêng
- Commit nhỏ, message rõ ràng
- Pull code mới nhất trước khi push

## Demo

- Chạy `MainForm`
- Duyệt thư mục trong `TreeView`/`ListView`
- Double-click file để mở bằng ứng dụng mặc định

## Ghi chú

- Test trong thư mục riêng, tránh thao tác trên dữ liệu thật quan trọng
- Luôn xử lý lỗi quyền truy cập (`UnauthorizedAccessException`) để tránh crash UI
