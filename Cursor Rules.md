# Mini File Manager — Cursor Rules
# Phần việc: Người 1 — UI & Điều hướng (Navigation)
# Công nghệ: C# + WinForms (.NET 6+)
# ============================================================

## 1. CẤU TRÚC DỰ ÁN BẮT BUỘC

```
MiniFileManager/
├── Forms/
│   ├── MainForm.cs          ← Form chính (Người 1 sở hữu)
│   ├── MainForm.Designer.cs
│   └── MainForm.resx
├── Services/
│   ├── IFileOperationService.cs   ← Interface do Người 1 định nghĩa
│   ├── INavigationService.cs      ← Interface do Người 1 định nghĩa
│   ├── BasicIOService.cs          ← Người 2 implement
│   ├── AdvancedIOService.cs       ← Người 3 implement
│   └── OSCoreService.cs           ← Người 4 implement
├── Models/
│   └── FileSystemItem.cs          ← Model dùng chung toàn nhóm
├── Helpers/
│   └── IconHelper.cs              ← Helper lấy icon file/folder
└── Program.cs
```

## 2. INTERFACE BẮT BUỘC (Người 1 định nghĩa — nhóm implement)

### IFileOperationService.cs
```csharp
// Người 2 & 3 implement interface này
// Người 1 chỉ DÙNG interface, không implement
public interface IFileOperationService
{
    // --- Basic I/O (Người 2) ---
    void CreateFolder(string parentPath, string folderName);
    void DeleteItem(string path);
    void RenameItem(string path, string newName);
    FileSystemItem GetProperties(string path);

    // --- Advanced I/O (Người 3) ---
    void CopyItem(string sourcePath, string destinationPath);
    void CutItem(string sourcePath, string destinationPath);
    void PasteItem();
    IEnumerable<string> SearchFiles(string rootPath, string pattern);
}
```

### INavigationService.cs
```csharp
// Người 1 tự implement NavigationService
public interface INavigationService
{
    string CurrentPath { get; }
    void NavigateTo(string path);
    void NavigateBack();
    void NavigateForward();
    void NavigateUp();
    event EventHandler<string> PathChanged;
}
```

### Models/FileSystemItem.cs
```csharp
// Model dùng chung — không được thay đổi tên property
public class FileSystemItem
{
    public string Name { get; set; }
    public string FullPath { get; set; }
    public bool IsDirectory { get; set; }
    public long Size { get; set; }             // bytes, 0 nếu là folder
    public DateTime LastModified { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Extension { get; set; }
    public bool IsHidden { get; set; }
}
```

## 3. QUY TẮC VIẾT CODE (áp dụng toàn bộ project)

### Naming Convention
- Class, Method, Property: PascalCase → `MainForm`, `NavigateTo()`, `CurrentPath`
- Private field: camelCase với underscore → `_currentPath`, `_fileService`
- Event handler: `Object_EventName` → `TreeView_BeforeExpand`, `ListView_DoubleClick`
- Interface: tiền tố "I" → `IFileOperationService`
- Không viết tắt khó hiểu: dùng `sourceDirectory` không phải `srcDir`

### Cấu trúc MainForm
```csharp
public partial class MainForm : Form
{
    // 1. Fields (inject service qua constructor)
    private readonly IFileOperationService _fileService;
    private readonly INavigationService _navigationService;

    // 2. Constructor — Dependency Injection thủ công
    public MainForm(IFileOperationService fileService)
    {
        InitializeComponent();
        _fileService = fileService;
        _navigationService = new NavigationService();
        InitializeUI();
        SubscribeEvents();
    }

    // 3. Phân chia method rõ ràng
    private void InitializeUI() { }          // setup ban đầu
    private void SubscribeEvents() { }       // đăng ký events
    private void LoadDrives() { }            // load ổ đĩa vào TreeView
    private void LoadDirectory(string path) { } // load nội dung thư mục
    private void RefreshListView() { }       // cập nhật ListView

    // 4. Event handlers ở cuối file
    private void TreeView_AfterSelect(...) { }
    private void ListView_DoubleClick(...) { }
}
```

## 4. QUY TẮC GIAO TIẾP GIỮA CÁC MODULE

### Người 1 KHÔNG được làm:
- Trực tiếp gọi `File.Delete()`, `File.Copy()`, `Directory.Move()` trong MainForm
- Xử lý buffer khi copy file lớn (đó là việc của Người 3)
- Gọi Win32 API trực tiếp (đó là việc của Người 4)

### Người 1 CHỈ được dùng:
```csharp
// Đọc thông tin để HIỂN THỊ (ok)
Directory.GetDirectories(path)
Directory.GetFiles(path)
new DirectoryInfo(path)
new DriveInfo(driveName)

// Gọi qua interface (ok)
_fileService.CopyItem(src, dst);
_fileService.DeleteItem(path);

// KHÔNG được gọi trực tiếp (sai)
File.Delete(path);      // ❌ — phải qua _fileService
File.Copy(src, dst);    // ❌ — phải qua _fileService
```

### Cách các thành viên khác tích hợp:
```csharp
// Người 2 & 3 tạo class implement IFileOperationService
// Người 1 nhận qua constructor — không cần sửa MainForm

// Ví dụ trong Program.cs
static class Program
{
    [STAThread]
    static void Main()
    {
        var fileService = new BasicIOService();      // Người 2 cung cấp
        // hoặc: var fileService = new AdvancedIOService(); // Người 3
        Application.Run(new MainForm(fileService));
    }
}
```

## 5. QUY TẮC UI COMPONENTS

### TreeView (bắt buộc)
```csharp
// Load lazy: chỉ load khi user expand
private void TreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
{
    var node = e.Node;
    if (node.Tag is string path)
    {
        node.Nodes.Clear(); // xóa node giả
        try
        {
            foreach (var dir in Directory.GetDirectories(path))
            {
                var child = new TreeNode(Path.GetFileName(dir)) { Tag = dir };
                child.Nodes.Add("..."); // node giả để có nút expand
                node.Nodes.Add(child);
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Hiển thị "(Không có quyền truy cập)" — không crash
            node.Nodes.Add(new TreeNode("(Không có quyền truy cập)") { ForeColor = Color.Gray });
        }
    }
}
```

### ListView (bắt buộc)
```csharp
// Bắt buộc có 4 cột này (Người 2 cần để hiển thị Properties)
listView.Columns.Add("Tên", 250);
listView.Columns.Add("Kích thước", 100);
listView.Columns.Add("Ngày sửa đổi", 150);
listView.Columns.Add("Loại", 80);

// Hiển thị từ FileSystemItem (model chung)
private ListViewItem CreateListViewItem(FileSystemItem item)
{
    var lvi = new ListViewItem(item.Name);
    lvi.SubItems.Add(item.IsDirectory ? "" : FormatSize(item.Size));
    lvi.SubItems.Add(item.LastModified.ToString("dd/MM/yyyy HH:mm"));
    lvi.SubItems.Add(item.IsDirectory ? "Thư mục" : item.Extension.ToUpper());
    lvi.Tag = item; // lưu model để các thành viên khác dùng
    return lvi;
}
```

## 6. XỬ LÝ LỖI BẮT BUỘC

```csharp
// Mọi thao tác truy cập file system phải có try-catch
private void LoadDirectory(string path)
{
    try
    {
        // ... load files
    }
    catch (UnauthorizedAccessException)
    {
        MessageBox.Show($"Không có quyền truy cập: {path}",
            "Lỗi quyền truy cập", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
    catch (DirectoryNotFoundException)
    {
        MessageBox.Show($"Thư mục không tồn tại: {path}",
            "Thư mục không tìm thấy", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
    catch (Exception ex)
    {
        // Log lỗi (Người 4 có thể mở rộng phần này)
        MessageBox.Show($"Lỗi không xác định: {ex.Message}",
            "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
```

## 7. EVENTS CÔNG KHAI (để các thành viên khác hook vào)

```csharp
// MainForm expose các event để Người 2/3/4 có thể hook
public event EventHandler<string> FileSelected;      // khi chọn file
public event EventHandler<string> FolderNavigated;   // khi vào thư mục
public event EventHandler RefreshRequested;          // yêu cầu refresh UI

// Người 2/3 gọi sau khi thực hiện thao tác I/O:
// mainForm.RefreshRequested += (s, e) => mainForm.RefreshListView();
```

## 8. CHECKLIST TRƯỚC KHI COMMIT

- [ ] Không có `File.Delete/Copy/Move` trực tiếp trong Forms/
- [ ] Tất cả truy cập file system có try-catch `UnauthorizedAccessException`
- [ ] `IFileOperationService` và `INavigationService` không thay đổi signature
- [ ] `FileSystemItem` model không thay đổi tên property
- [ ] TreeView load lazy (không load toàn bộ cây ngay lúc khởi động)
- [ ] ListView có đủ 4 cột: Tên, Kích thước, Ngày sửa đổi, Loại
- [ ] `ListViewItem.Tag` luôn chứa object `FileSystemItem`
- [ ] Double-click vào folder → điều hướng (không mở file)
- [ ] Double-click vào file → gọi `Process.Start()` mở bằng app mặc định
- [ ] StatusBar cập nhật đường dẫn hiện tại sau mỗi lần điều hướng

## 9. GHI CHÚ CHO CURSOR AI

Khi được yêu cầu viết code cho project này:
- Luôn tuân thủ cấu trúc thư mục mục 1
- Luôn dùng `IFileOperationService` thay vì gọi `System.IO` trực tiếp trong Forms
- Khi tạo method mới trong MainForm, đặt đúng vị trí theo phân nhóm (Init / Events / Helpers)
- Không dùng `async/await` ở tầng UI trừ khi có yêu cầu rõ ràng
- Comment tiếng Việt được chấp nhận cho phần business logic
- Không hardcode đường dẫn (dùng `Environment.GetFolderPath()` hoặc nhận từ user)
# Mini File Manager — Cursor Rules BỔ SUNG
# Người 1 — UI & Điều hướng (Navigation)
# Bổ sung vào file cursor rules gốc
# ============================================================

## 10. NAVIGATIONSERVICE — IMPLEMENTATION BẮT BUỘC

```csharp
// Services/NavigationService.cs — Người 1 tự implement
public class NavigationService : INavigationService
{
    private readonly Stack<string> _backStack = new();
    private readonly Stack<string> _forwardStack = new();
    private string _currentPath = string.Empty;

    public string CurrentPath => _currentPath;

    public event EventHandler<string>? PathChanged;

    public void NavigateTo(string path)
    {
        if (!Directory.Exists(path)) return;
        if (_currentPath == path) return;

        if (!string.IsNullOrEmpty(_currentPath))
            _backStack.Push(_currentPath);

        _forwardStack.Clear(); // xóa forward stack khi điều hướng mới
        _currentPath = path;
        PathChanged?.Invoke(this, _currentPath);
    }

    public void NavigateBack()
    {
        if (_backStack.Count == 0) return;
        _forwardStack.Push(_currentPath);
        _currentPath = _backStack.Pop();
        PathChanged?.Invoke(this, _currentPath);
    }

    public void NavigateForward()
    {
        if (_forwardStack.Count == 0) return;
        _backStack.Push(_currentPath);
        _currentPath = _forwardStack.Pop();
        PathChanged?.Invoke(this, _currentPath);
    }

    public void NavigateUp()
    {
        var parent = Directory.GetParent(_currentPath);
        if (parent != null)
            NavigateTo(parent.FullName);
    }

    // Helper để MainForm biết có thể Back/Forward không
    public bool CanGoBack => _backStack.Count > 0;
    public bool CanGoForward => _forwardStack.Count > 0;
}
```

---

## 11. MOCK SERVICE — CHẠY APP KHÔNG CẦN CHỜ NGƯỜI 2/3

```csharp
// Services/MockFileOperationService.cs
// Người 1 tạo class này để test UI độc lập
// Khi Người 2/3 hoàn thành → xóa file này, swap trong Program.cs

public class MockFileOperationService : IFileOperationService
{
    // --- Basic I/O (Người 2 sẽ implement thật) ---
    public void CreateFolder(string parentPath, string folderName)
        => MessageBox.Show($"[MOCK] Tạo folder: {folderName} trong {parentPath}");

    public void DeleteItem(string path)
        => MessageBox.Show($"[MOCK] Xóa: {path}");

    public void RenameItem(string path, string newName)
        => MessageBox.Show($"[MOCK] Đổi tên: {path} → {newName}");

    public FileSystemItem GetProperties(string path)
    {
        // Trả về dữ liệu giả để UI không crash
        var info = new FileInfo(path);
        return new FileSystemItem
        {
            Name = info.Name,
            FullPath = path,
            IsDirectory = false,
            Size = 0,
            LastModified = DateTime.Now,
            CreatedDate = DateTime.Now,
            Extension = info.Extension,
            IsHidden = false
        };
    }

    // --- Advanced I/O (Người 3 sẽ implement thật) ---
    public void CopyItem(string sourcePath, string destinationPath)
        => MessageBox.Show($"[MOCK] Copy: {sourcePath} → {destinationPath}");

    public void CutItem(string sourcePath, string destinationPath)
        => MessageBox.Show($"[MOCK] Cut: {sourcePath}");

    public void PasteItem()
        => MessageBox.Show("[MOCK] Paste");

    public IEnumerable<string> SearchFiles(string rootPath, string pattern)
    {
        // Trả về danh sách rỗng để UI không crash
        return Enumerable.Empty<string>();
    }
}
```

---

## 12. ADDRESS BAR & BREADCRUMB

```csharp
// Trong MainForm.cs — Address bar + Breadcrumb
// Thêm vào InitializeUI()

// ToolStrip chứa các nút điều hướng + ô địa chỉ
private ToolStrip _navigationToolStrip;
private ToolStripButton _btnBack;
private ToolStripButton _btnForward;
private ToolStripButton _btnUp;
private ToolStripButton _btnRefresh;
private ToolStripTextBox _addressBar;

private void InitializeNavigationBar()
{
    _btnBack    = new ToolStripButton("◄") { ToolTipText = "Quay lại", Enabled = false };
    _btnForward = new ToolStripButton("►") { ToolTipText = "Tiến tới", Enabled = false };
    _btnUp      = new ToolStripButton("▲") { ToolTipText = "Lên thư mục cha" };
    _btnRefresh = new ToolStripButton("↺") { ToolTipText = "Làm mới" };
    _addressBar = new ToolStripTextBox { Width = 400 };

    _btnBack.Click    += (s, e) => _navigationService.NavigateBack();
    _btnForward.Click += (s, e) => _navigationService.NavigateForward();
    _btnUp.Click      += (s, e) => _navigationService.NavigateUp();
    _btnRefresh.Click += (s, e) => LoadDirectory(_navigationService.CurrentPath);
    _addressBar.KeyDown += AddressBar_KeyDown;

    _navigationToolStrip.Items.AddRange(new ToolStripItem[]
    {
        _btnBack, _btnForward, _btnUp,
        new ToolStripSeparator(),
        _btnRefresh,
        new ToolStripSeparator(),
        _addressBar
    });
}

private void AddressBar_KeyDown(object? sender, KeyEventArgs e)
{
    if (e.KeyCode == Keys.Enter)
    {
        var path = _addressBar.Text.Trim();
        if (Directory.Exists(path))
            _navigationService.NavigateTo(path);
        else
            MessageBox.Show("Đường dẫn không tồn tại.", "Lỗi",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        e.Handled = true;
        e.SuppressKeyPress = true;
    }
}

// Cập nhật address bar & nút Back/Forward khi đường dẫn thay đổi
private void NavigationService_PathChanged(object? sender, string newPath)
{
    _addressBar.Text = newPath;
    _btnBack.Enabled    = _navigationService.CanGoBack;
    _btnForward.Enabled = _navigationService.CanGoForward;
    LoadDirectory(newPath);
    SyncTreeViewSelection(newPath); // cuộn TreeView đến node tương ứng
}
```

---

## 13. STATUSBAR — ĐẶC TẢ BẮT BUỘC

```csharp
// Trong MainForm.cs — StatusBar phải có đủ 3 phần

private StatusStrip _statusStrip;
private ToolStripStatusLabel _lblCurrentPath;   // hiện đường dẫn
private ToolStripStatusLabel _lblItemCount;     // số file/folder
private ToolStripStatusLabel _lblSelectedInfo;  // thông tin mục đang chọn

private void InitializeStatusBar()
{
    _lblCurrentPath  = new ToolStripStatusLabel { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
    _lblItemCount    = new ToolStripStatusLabel { BorderSides = ToolStripStatusLabelBorderSides.Left };
    _lblSelectedInfo = new ToolStripStatusLabel { BorderSides = ToolStripStatusLabelBorderSides.Left };

    _statusStrip.Items.AddRange(new ToolStripItem[]
    {
        _lblCurrentPath, _lblItemCount, _lblSelectedInfo
    });
}

// Gọi sau mỗi lần LoadDirectory()
private void UpdateStatusBar(string path, int totalItems, int selectedCount = 0)
{
    _lblCurrentPath.Text  = path;
    _lblItemCount.Text    = $"{totalItems} mục";
    _lblSelectedInfo.Text = selectedCount > 0
        ? $"Đã chọn: {selectedCount} mục"
        : string.Empty;
}
```

---

## 14. ICONHELPER — HIỂN THỊ ICON FILE/FOLDER

```csharp
// Helpers/IconHelper.cs
public static class IconHelper
{
    private static readonly ImageList _imageList = new() { ImageSize = new Size(16, 16) };

    // Key quy ước — các thành viên khác dùng cùng key này
    public const string KeyFolder     = "folder";
    public const string KeyFolderOpen = "folder_open";
    public const string KeyFile       = "file";
    public const string KeyDrive      = "drive";

    public static ImageList GetImageList()
    {
        if (_imageList.Images.Count > 0) return _imageList;

        // Lấy icon từ hệ thống (không cần file ảnh ngoài)
        _imageList.Images.Add(KeyFolder,     SystemIcons.Information.ToBitmap()); // thay bằng icon folder thật
        _imageList.Images.Add(KeyFolderOpen, SystemIcons.Information.ToBitmap());
        _imageList.Images.Add(KeyFile,       SystemIcons.WinLogo.ToBitmap());
        _imageList.Images.Add(KeyDrive,      SystemIcons.Shield.ToBitmap());

        return _imageList;
    }

    // Lấy icon theo extension — dùng Shell để lấy icon chính xác
    public static Icon? GetFileIcon(string path)
    {
        try
        {
            return Icon.ExtractAssociatedIcon(path);
        }
        catch
        {
            return SystemIcons.WinLogo;
        }
    }

    // Thêm icon của extension vào ImageList nếu chưa có
    public static string EnsureExtensionIcon(ImageList imageList, string extension)
    {
        var key = extension.ToLower();
        if (!imageList.Images.ContainsKey(key))
        {
            // Tạo file tạm để lấy icon
            var tempFile = Path.Combine(Path.GetTempPath(), $"icontemp{extension}");
            try
            {
                File.WriteAllText(tempFile, "");
                var icon = Icon.ExtractAssociatedIcon(tempFile);
                if (icon != null)
                    imageList.Images.Add(key, icon.ToBitmap());
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }
        return key;
    }
}
```

---

## 15. SYNCTREEVIEW — ĐỒNG BỘ TREEVIEW VỚI LISTVIEW

```csharp
// Trong MainForm.cs — đảm bảo TreeView highlight đúng node
// khi user gõ địa chỉ vào Address Bar hoặc double-click ở ListView

private void SyncTreeViewSelection(string path)
{
    // Tìm node khớp với path trong TreeView
    var node = FindNodeByPath(treeView.Nodes, path);
    if (node != null)
    {
        treeView.SelectedNode = node;
        treeView.SelectedNode.EnsureVisible();
    }
}

private TreeNode? FindNodeByPath(TreeNodeCollection nodes, string path)
{
    foreach (TreeNode node in nodes)
    {
        if (node.Tag is string nodePath &&
            string.Equals(nodePath, path, StringComparison.OrdinalIgnoreCase))
            return node;

        // Tìm đệ quy trong node con đã load
        var found = FindNodeByPath(node.Nodes, path);
        if (found != null) return found;
    }
    return null;
}
```

---

## 16. PROGRAM.CS — CÁCH SWAP SERVICE KHI NHÓM HOÀN THÀNH

```csharp
// Program.cs — điểm tích hợp duy nhất
// Người 1 dùng Mock, sau đó Người 2/3 chỉ đổi 1 dòng này

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // ====================================================
        // ĐỔI SERVICE Ở ĐÂY KHI NHÓM HOÀN THÀNH
        // Giai đoạn dev (Người 1 làm một mình):
        IFileOperationService fileService = new MockFileOperationService();

        // Khi Người 2 xong Basic I/O → đổi thành:
        // IFileOperationService fileService = new BasicIOService();

        // Khi Người 3 xong Advanced I/O → đổi thành:
        // IFileOperationService fileService = new AdvancedIOService();
        // ====================================================

        Application.Run(new MainForm(fileService));
    }
}
```

---

## 17. CONTEXT MENU — ĐẦU NỐI VỚI NGƯỜI 2/3/4

```csharp
// MainForm.cs — ContextMenuStrip cho ListView
// Người 1 tạo menu, gọi qua _fileService (interface)
// Người 2/3 implement phần xử lý thật

private ContextMenuStrip _contextMenu;

private void InitializeContextMenu()
{
    _contextMenu = new ContextMenuStrip();

    var mnuOpen       = new ToolStripMenuItem("Mở");
    var mnuCopy       = new ToolStripMenuItem("Sao chép\tCtrl+C");
    var mnuCut        = new ToolStripMenuItem("Cắt\tCtrl+X");
    var mnuPaste      = new ToolStripMenuItem("Dán\tCtrl+V");
    var mnuDelete     = new ToolStripMenuItem("Xóa\tDel");
    var mnuRename     = new ToolStripMenuItem("Đổi tên\tF2");
    var mnuNewFolder  = new ToolStripMenuItem("Thư mục mới");
    var mnuProperties = new ToolStripMenuItem("Thuộc tính");

    // Người 1 gọi qua interface — KHÔNG gọi File.* trực tiếp
    mnuCopy.Click       += (s, e) => CopySelectedItem();
    mnuCut.Click        += (s, e) => CutSelectedItem();
    mnuPaste.Click      += (s, e) => _fileService.PasteItem();
    mnuDelete.Click     += (s, e) => DeleteSelectedItem();
    mnuRename.Click     += (s, e) => RenameSelectedItem();
    mnuNewFolder.Click  += (s, e) => CreateNewFolder();
    mnuProperties.Click += (s, e) => ShowProperties();

    _contextMenu.Items.AddRange(new ToolStripItem[]
    {
        mnuOpen,
        new ToolStripSeparator(),
        mnuCopy, mnuCut, mnuPaste,
        new ToolStripSeparator(),
        mnuDelete, mnuRename,
        new ToolStripSeparator(),
        mnuNewFolder,
        new ToolStripSeparator(),
        mnuProperties
    });

    listView.ContextMenuStrip = _contextMenu;
}

// Các method helper — Người 1 viết phần gọi service
// Người 2/3 implement logic trong service
private void CopySelectedItem()
{
    if (listView.SelectedItems.Count == 0) return;
    if (listView.SelectedItems[0].Tag is FileSystemItem item)
        _fileService.CopyItem(item.FullPath, string.Empty); // dest sẽ set khi Paste
}

private void DeleteSelectedItem()
{
    if (listView.SelectedItems.Count == 0) return;
    if (listView.SelectedItems[0].Tag is FileSystemItem item)
    {
        var confirm = MessageBox.Show($"Xóa '{item.Name}'?", "Xác nhận xóa",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        if (confirm == DialogResult.Yes)
        {
            _fileService.DeleteItem(item.FullPath);
            RefreshListView();
        }
    }
}

private void ShowProperties()
{
    if (listView.SelectedItems.Count == 0) return;
    if (listView.SelectedItems[0].Tag is FileSystemItem item)
    {
        var props = _fileService.GetProperties(item.FullPath);
        // Người 1 có thể tạo PropertyForm đơn giản hoặc dùng MessageBox
        MessageBox.Show(
            $"Tên: {props.Name}\n" +
            $"Đường dẫn: {props.FullPath}\n" +
            $"Kích thước: {FormatSize(props.Size)}\n" +
            $"Ngày tạo: {props.CreatedDate:dd/MM/yyyy HH:mm}\n" +
            $"Ngày sửa: {props.LastModified:dd/MM/yyyy HH:mm}",
            "Thuộc tính", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
```

---

## 18. KEYBOARD SHORTCUTS BẮT BUỘC

```csharp
// MainForm.cs — Override ProcessCmdKey để bắt phím tắt
protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
{
    switch (keyData)
    {
        case Keys.Alt | Keys.Left:  _navigationService.NavigateBack();    return true;
        case Keys.Alt | Keys.Right: _navigationService.NavigateForward(); return true;
        case Keys.Alt | Keys.Up:    _navigationService.NavigateUp();      return true;
        case Keys.F5:               LoadDirectory(_navigationService.CurrentPath); return true;
        case Keys.F2:               RenameSelectedItem(); return true;
        case Keys.Delete:           DeleteSelectedItem(); return true;
        case Keys.Control | Keys.C: CopySelectedItem();  return true;
        case Keys.Control | Keys.X: CutSelectedItem();   return true;
        case Keys.Control | Keys.V: _fileService.PasteItem(); return true;
    }
    return base.ProcessCmdKey(ref msg, keyData);
}
```

---

## 19. CHECKLIST BỔ SUNG (thêm vào mục 8 gốc)

- [ ] `NavigationService` có đủ Back/Forward/Up stack hoạt động đúng
- [ ] Address bar đồng bộ 2 chiều với TreeView (gõ địa chỉ → TreeView scroll đến node)
- [ ] `CanGoBack` / `CanGoForward` điều khiển Enable/Disable nút toolbar
- [ ] Context menu đầy đủ 8 mục: Open, Copy, Cut, Paste, Delete, Rename, New Folder, Properties
- [ ] Keyboard shortcuts hoạt động: Alt+←/→/↑, F5, F2, Del, Ctrl+C/X/V
- [ ] `MockFileOperationService` tồn tại để Người 1 test UI độc lập
- [ ] `Program.cs` có comment rõ nơi swap service khi Người 2/3 hoàn thành
- [ ] `StatusBar` cập nhật số mục sau mỗi lần load directory
- [ ] `IconHelper.GetImageList()` được gán cho cả TreeView lẫn ListView
- [ ] Double-click file → `Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true })`