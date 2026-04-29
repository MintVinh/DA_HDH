using System.Drawing;

namespace MiniFileManager.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null!;
    private SplitContainer _splitContainer = null!;
    private TreeView _treeView = null!;
    private ListView _listView = null!;
    private ToolStrip _toolStrip = null!;
    private ToolStripButton _btnBack = null!;
    private ToolStripButton _btnForward = null!;
    private ToolStripButton _btnUp = null!;
    private ToolStripButton _btnRefresh = null!;
    private StatusStrip _statusStrip = null!;
    private ToolStripStatusLabel _lblCurrentPath = null!;
    private ToolStripStatusLabel _lblItemCount = null!;
    private ToolStripStatusLabel _lblSelectedInfo = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _splitContainer = new SplitContainer();
        _treeView = new TreeView();
        _listView = new ListView();
        _toolStrip = new ToolStrip();
        _btnBack = new ToolStripButton();
        _btnForward = new ToolStripButton();
        _btnUp = new ToolStripButton();
        _btnRefresh = new ToolStripButton();
        _statusStrip = new StatusStrip();
        _lblCurrentPath = new ToolStripStatusLabel();
        _lblItemCount = new ToolStripStatusLabel();
        _lblSelectedInfo = new ToolStripStatusLabel();

        ((System.ComponentModel.ISupportInitialize)_splitContainer).BeginInit();
        _splitContainer.Panel1.SuspendLayout();
        _splitContainer.Panel2.SuspendLayout();
        _splitContainer.SuspendLayout();
        _toolStrip.SuspendLayout();
        _statusStrip.SuspendLayout();
        SuspendLayout();

        _splitContainer.Dock = DockStyle.Fill;
        _splitContainer.Location = new Point(0, 25);
        _splitContainer.Name = "_splitContainer";
        _splitContainer.SplitterDistance = 280;
        _splitContainer.TabIndex = 0;

        _treeView.Dock = DockStyle.Fill;
        _treeView.Location = new Point(0, 0);
        _treeView.Name = "_treeView";
        _treeView.TabIndex = 0;

        _listView.Dock = DockStyle.Fill;
        _listView.Location = new Point(0, 0);
        _listView.Name = "_listView";
        _listView.TabIndex = 0;
        _listView.UseCompatibleStateImageBehavior = false;
        _listView.View = View.Details;
        _listView.FullRowSelect = true;

        _toolStrip.GripStyle = ToolStripGripStyle.Hidden;
        _toolStrip.Items.AddRange(new ToolStripItem[] { _btnBack, _btnForward, _btnUp, _btnRefresh });
        _toolStrip.Location = new Point(0, 0);
        _toolStrip.Name = "_toolStrip";
        _toolStrip.Size = new Size(900, 25);
        _toolStrip.TabIndex = 1;

        _btnBack.DisplayStyle = ToolStripItemDisplayStyle.Text;
        _btnBack.Name = "_btnBack";
        _btnBack.Size = new Size(41, 22);
        _btnBack.Text = "Back";

        _btnForward.DisplayStyle = ToolStripItemDisplayStyle.Text;
        _btnForward.Name = "_btnForward";
        _btnForward.Size = new Size(56, 22);
        _btnForward.Text = "Forward";

        _btnUp.DisplayStyle = ToolStripItemDisplayStyle.Text;
        _btnUp.Name = "_btnUp";
        _btnUp.Size = new Size(26, 22);
        _btnUp.Text = "Up";

        _btnRefresh.DisplayStyle = ToolStripItemDisplayStyle.Text;
        _btnRefresh.Name = "_btnRefresh";
        _btnRefresh.Size = new Size(50, 22);
        _btnRefresh.Text = "Refresh";

        _statusStrip.Items.AddRange(new ToolStripItem[] { _lblCurrentPath, _lblItemCount, _lblSelectedInfo });
        _statusStrip.Location = new Point(0, 578);
        _statusStrip.Name = "_statusStrip";
        _statusStrip.Size = new Size(900, 22);
        _statusStrip.TabIndex = 2;

        _lblCurrentPath.Name = "_lblCurrentPath";
        _lblCurrentPath.Size = new Size(0, 17);
        _lblCurrentPath.Spring = true;
        _lblCurrentPath.TextAlign = ContentAlignment.MiddleLeft;

        _lblItemCount.Name = "_lblItemCount";
        _lblItemCount.Size = new Size(0, 17);
        _lblItemCount.BorderSides = ToolStripStatusLabelBorderSides.Left;

        _lblSelectedInfo.Name = "_lblSelectedInfo";
        _lblSelectedInfo.Size = new Size(0, 17);
        _lblSelectedInfo.BorderSides = ToolStripStatusLabelBorderSides.Left;

        _splitContainer.Panel1.Controls.Add(_treeView);
        _splitContainer.Panel2.Controls.Add(_listView);
        Controls.Add(_toolStrip);
        Controls.Add(_splitContainer);
        Controls.Add(_statusStrip);

        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(900, 600);
        Name = "MainForm";
        Text = "MiniFileManager";
        StartPosition = FormStartPosition.CenterScreen;

        _splitContainer.Panel1.ResumeLayout(false);
        _splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_splitContainer).EndInit();
        _splitContainer.ResumeLayout(false);
        _toolStrip.ResumeLayout(false);
        _toolStrip.PerformLayout();
        _statusStrip.ResumeLayout(false);
        _statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
