using System.Diagnostics;
using MiniFileManager.Models;
using MiniFileManager.Services;

namespace MiniFileManager.Forms;

public partial class MainForm : Form
{
    private readonly IFileOperationService _fileService;
    private readonly INavigationService _navigationService;

    public MainForm(IFileOperationService fileService)
    {
        InitializeComponent();
        _fileService = fileService;
        _navigationService = new NavigationService();
        InitializeUI();
        SubscribeEvents();
    }

    private void InitializeUI()
    {
        var mainBackground = ColorTranslator.FromHtml("#1E1E1E");
        var panelBackground = ColorTranslator.FromHtml("#252526");
        var foreground = ColorTranslator.FromHtml("#FFFFFF");
        var accent = ColorTranslator.FromHtml("#007ACC");

        Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
        BackColor = mainBackground;
        ForeColor = foreground;
        Padding = new Padding(8);

        _toolStrip.BackColor = panelBackground;
        _toolStrip.ForeColor = foreground;
        _toolStrip.Padding = new Padding(6, 2, 6, 2);
        _toolStrip.RenderMode = ToolStripRenderMode.System;
        _btnBack.ForeColor = foreground;
        _btnForward.ForeColor = foreground;
        _btnUp.ForeColor = foreground;
        _btnRefresh.ForeColor = foreground;

        _splitContainer.BackColor = mainBackground;
        _splitContainer.Panel1.BackColor = panelBackground;
        _splitContainer.Panel2.BackColor = panelBackground;
        _splitContainer.Panel1.Padding = new Padding(8, 8, 4, 8);
        _splitContainer.Panel2.Padding = new Padding(4, 8, 8, 8);
        _splitContainer.SplitterWidth = 6;
        _splitContainer.SplitterDistance = (int)(ClientSize.Width * 0.3);

        _treeView.BackColor = panelBackground;
        _treeView.ForeColor = foreground;
        _treeView.FullRowSelect = true;
        _treeView.HideSelection = false;
        _treeView.BorderStyle = BorderStyle.None;
        _treeView.LineColor = accent;

        _listView.BackColor = panelBackground;
        _listView.ForeColor = foreground;
        _listView.FullRowSelect = true;
        _listView.GridLines = false;
        _listView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        _listView.BorderStyle = BorderStyle.None;
        _listView.View = View.Details;
        _listView.Columns.Clear();
        _listView.Columns.Add("Tên", 250);
        _listView.Columns.Add("Kích thước", 100);
        _listView.Columns.Add("Ngày sửa đổi", 150);
        _listView.Columns.Add("Loại", 100);

        _statusStrip.BackColor = panelBackground;
        _statusStrip.ForeColor = foreground;
        _statusStrip.SizingGrip = false;
        _lblCurrentPath.ForeColor = foreground;
        _lblCurrentPath.Text = "CurrentPath: ";
        _lblItemCount.ForeColor = foreground;
        _lblSelectedInfo.ForeColor = foreground;

        LoadDrives();
        _lblItemCount.Text = string.Empty;
        _lblSelectedInfo.Text = string.Empty;
    }

    private void SubscribeEvents()
    {
        _navigationService.PathChanged += NavigationService_PathChanged;
        _treeView.BeforeExpand += TreeView_BeforeExpand;
        _treeView.AfterSelect += TreeView_AfterSelect;
        _listView.DoubleClick += ListView_DoubleClick;
    }

    private void LoadDrives()
    {
        _treeView.Nodes.Clear();

        foreach (var drive in DriveInfo.GetDrives())
        {
            if (!drive.IsReady)
            {
                continue;
            }

            var drivePath = drive.RootDirectory.FullName;
            var node = new TreeNode(drive.Name)
            {
                Tag = drivePath
            };

            // Node giả để hiển thị nút expand, dữ liệu thật sẽ load khi cần.
            node.Nodes.Add("...");
            _treeView.Nodes.Add(node);
        }
    }

    private void LoadDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            var items = new List<FileSystemItem>();

            foreach (var directoryPath in Directory.GetDirectories(path))
            {
                var directoryInfo = new DirectoryInfo(directoryPath);
                items.Add(new FileSystemItem
                {
                    Name = directoryInfo.Name,
                    FullPath = directoryPath,
                    IsDirectory = true,
                    Size = 0,
                    LastModified = directoryInfo.LastWriteTime,
                    CreatedDate = directoryInfo.CreationTime,
                    Extension = string.Empty,
                    IsHidden = directoryInfo.Attributes.HasFlag(FileAttributes.Hidden)
                });
            }

            foreach (var filePath in Directory.GetFiles(path))
            {
                var fileInfo = new FileInfo(filePath);
                items.Add(new FileSystemItem
                {
                    Name = fileInfo.Name,
                    FullPath = filePath,
                    IsDirectory = false,
                    Size = fileInfo.Length,
                    LastModified = fileInfo.LastWriteTime,
                    CreatedDate = fileInfo.CreationTime,
                    Extension = fileInfo.Extension,
                    IsHidden = fileInfo.Attributes.HasFlag(FileAttributes.Hidden)
                });
            }

            _listView.Items.Clear();
            foreach (var item in items)
            {
                _listView.Items.Add(CreateListViewItem(item));
            }

            _lblCurrentPath.Text = $"CurrentPath: {path}";
            _lblItemCount.Text = $"Items: {items.Count}";
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
            MessageBox.Show($"Lỗi không xác định: {ex.Message}",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RefreshListView()
    {
    }

    private void NavigationService_PathChanged(object? sender, string newPath)
    {
        _lblCurrentPath.Text = $"CurrentPath: {newPath}";
        LoadDirectory(newPath);
    }

    private ListViewItem CreateListViewItem(FileSystemItem item)
    {
        var listViewItem = new ListViewItem(item.Name);
        listViewItem.SubItems.Add(item.IsDirectory ? string.Empty : FormatSize(item.Size));
        listViewItem.SubItems.Add(item.LastModified.ToString("dd/MM/yyyy HH:mm"));
        listViewItem.SubItems.Add(item.IsDirectory ? "Thư mục" : item.Extension.ToUpperInvariant());
        listViewItem.Tag = item;
        return listViewItem;
    }

    private static string FormatSize(long sizeInBytes)
    {
        if (sizeInBytes < 1024)
        {
            return $"{sizeInBytes} B";
        }

        double size = sizeInBytes;
        var units = new[] { "KB", "MB", "GB", "TB" };
        var unitIndex = -1;

        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{size:0.##} {units[unitIndex]}";
    }

    private void TreeView_BeforeExpand(object? sender, TreeViewCancelEventArgs e)
    {
        var node = e.Node;
        if (node == null)
        {
            return;
        }

        if (node.Tag is not string path)
        {
            return;
        }

        if (node.Nodes.Count == 1 && node.Nodes[0].Text == "...")
        {
            node.Nodes.Clear();
        }

        try
        {
            foreach (var directoryPath in Directory.GetDirectories(path))
            {
                var childNode = new TreeNode(Path.GetFileName(directoryPath))
                {
                    Tag = directoryPath
                };

                childNode.Nodes.Add("...");
                node.Nodes.Add(childNode);
            }
        }
        catch (UnauthorizedAccessException)
        {
            node.Nodes.Clear();
            node.Nodes.Add(new TreeNode("(Không có quyền truy cập)") { ForeColor = Color.Gray });
        }
        catch
        {
            // Nuốt lỗi để UI không bị crash khi expand node.
        }
    }

    private void TreeView_AfterSelect(object? sender, TreeViewEventArgs e)
    {
        if (e.Node == null)
        {
            return;
        }

        if (e.Node.Tag is not string path || string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        _navigationService.NavigateTo(path);
        LoadDirectory(path);
        _lblCurrentPath.Text = $"CurrentPath: {path}";
    }

    private void ListView_DoubleClick(object? sender, EventArgs e)
    {
        if (_listView.SelectedItems.Count == 0)
        {
            return;
        }

        if (_listView.SelectedItems[0].Tag is not FileSystemItem item)
        {
            return;
        }

        if (item.IsDirectory)
        {
            _navigationService.NavigateTo(item.FullPath);
            LoadDirectory(item.FullPath);
            _lblCurrentPath.Text = $"CurrentPath: {item.FullPath}";
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = item.FullPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Không thể mở file: {ex.Message}",
                "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
