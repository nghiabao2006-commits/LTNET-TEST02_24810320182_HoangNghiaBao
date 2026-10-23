
namespace LTNET.Test02.Hospital;

public sealed class frmHospitalMain : Form
{
    private readonly HospitalData data = new();
    private readonly ToolStripStatusLabel lblOfficer = new() { Text = "Bác sĩ trực: Nguyễn Minh Anh (phiên mô phỏng)", Spring = true, TextAlign = ContentAlignment.MiddleLeft };
    private readonly ToolStripProgressBar progress = new() { Name = "tsProgress", Minimum = 0, Maximum = 100 };
    private readonly ToolStripMenuItem manage = new("Quản lý");
    private readonly ToolStripMenuItem reload = new("Nạp lại dữ liệu");
    private readonly ToolStripMenuItem logout = new("Đăng xuất");
    private readonly ToolStripMenuItem login = new("Đăng nhập");
    private bool signedIn = true;
    private CancellationTokenSource? reloadCancellation;

    public frmHospitalMain()
    {
        Text = "Bài 3 – Hệ thống quản lý bệnh viện MDI";
        Size = new Size(1250, 800);
        MinimumSize = new Size(850, 560);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10);
        AutoScaleMode = AutoScaleMode.Dpi;
        Name = "frmHospitalMain";
        IsMdiContainer = true;
        var menu = new MenuStrip();
        var system = new ToolStripMenuItem("Hệ thống");
        login.Enabled = false;
        login.Click += (_, _) => Login();
        logout.Click += (_, _) => Logout();
        reload.Click += async (_, _) => await ReloadAsync();
        system.DropDownItems.AddRange([login, logout, reload, new ToolStripSeparator()]);
        system.DropDownItems.Add("Thoát", null, (_, _) => Close());
        manage.DropDownItems.Add("Hồ sơ Bệnh nhân", null, (_, _) => OpenChild(() => new frmPatientProfiles(data)));
        manage.DropDownItems.Add("Danh mục Bác sĩ / Lịch trực", null, (_, _) => OpenChild(() => new frmDoctorSchedule(data)));
        manage.DropDownItems.Add("Danh mục Khoa/Phòng", null, (_, _) => OpenChild(() => new frmDepartmentCatalog(data)));
        var windows = new ToolStripMenuItem("Cửa sổ");
        windows.DropDownItems.Add("Xếp tầng (Cascade)", null, (_, _) => LayoutMdi(MdiLayout.Cascade));
        windows.DropDownItems.Add("Ngang (Tile Horizontal)", null, (_, _) => LayoutMdi(MdiLayout.TileHorizontal));
        windows.DropDownItems.Add("Dọc (Tile Vertical)", null, (_, _) => LayoutMdi(MdiLayout.TileVertical));
        windows.DropDownItems.Add("Đóng tất cả", null, (_, _) => CloseChildren());
        menu.Items.AddRange([system, manage, windows]);
        menu.MdiWindowListItem = windows;
        MainMenuStrip = menu;
        var status = new StatusStrip(); status.Items.AddRange([lblOfficer, progress]);
        Controls.Add(status); Controls.Add(menu);
        Shown += (_, _) => OpenChild(() => new frmPatientProfiles(data));
        FormClosing += (_, _) => reloadCancellation?.Cancel();
    }

    private void OpenChild<T>(Func<T> create) where T : Form
    {
        if (!signedIn || reloadCancellation is not null) return;
        // Explicitly inspect MdiChildren to prevent duplicate instances of each child type.
        foreach (Form child in MdiChildren)
        {
            if (child is not T) continue;
            if (child.WindowState == FormWindowState.Minimized) child.WindowState = FormWindowState.Normal;
            child.Activate();
            return;
        }
        var form = create();
        form.MdiParent = this;
        form.Show();
    }

    private void CloseChildren()
    {
        foreach (var child in MdiChildren) child.Close();
    }

    private void Logout()
    {
        if (MessageBox.Show(this, "Đăng xuất phiên làm việc hiện tại?", "Đăng xuất", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        CloseChildren();
        signedIn = false;
        manage.Enabled = reload.Enabled = logout.Enabled = false;
        login.Enabled = true;
        lblOfficer.Text = "Đã đăng xuất – chọn Hệ thống > Đăng nhập để mở phiên mô phỏng.";
    }

    private void Login()
    {
        string? officer = ShowInputDialog("Phiên làm việc mô phỏng", "Nhập tên bác sĩ/sĩ quan trực (không dùng mật khẩu):");
        if (officer is null) return;
        if (officer.Length == 0) { MessageBox.Show(this, "Tên người trực không được trống.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        signedIn = true;
        manage.Enabled = reload.Enabled = logout.Enabled = true;
        login.Enabled = false;
        lblOfficer.Text = "Bác sĩ/Sĩ quan trực: " + officer;
    }

    private async Task ReloadAsync()
    {
        if (reloadCancellation is not null) return;
        if (MessageBox.Show(this, "Khôi phục dữ liệu mẫu? Thay đổi trong bộ nhớ sẽ bị xóa.", "Nạp lại", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        using var cancellation = new CancellationTokenSource();
        reloadCancellation = cancellation;
        reload.Enabled = manage.Enabled = logout.Enabled = false;
        try
        {
            for (int i = 0; i <= 100; i += 10)
            {
                await Task.Delay(80, cancellation.Token);
                progress.Value = i;
            }
            CloseChildren();
            data.Reset();
            // Clear the in-flight guard before opening the default child.
            reloadCancellation = null;
            OpenChild(() => new frmPatientProfiles(data));
        }
        catch (OperationCanceledException) { }
        finally
        {
            reloadCancellation = null;
            if (!IsDisposed && !Disposing)
            {
                progress.Value = 0;
                reload.Enabled = manage.Enabled = logout.Enabled = signedIn;
            }
        }
    }

    private string? ShowInputDialog(string title, string caption)
    {
        using var dialog = new Form
        {
            Text = title, StartPosition = FormStartPosition.CenterParent,
            ClientSize = new Size(440, 150), FormBorderStyle = FormBorderStyle.FixedDialog,
            MinimizeBox = false, MaximizeBox = false
        };
        var label = new Label { Text = caption, Left = 15, Top = 15, AutoSize = true };
        var input = new TextBox { Left = 15, Top = 45, Width = 410 };
        var ok = new Button { Text = "Đồng ý", Left = 235, Top = 95, DialogResult = DialogResult.OK };
        var cancel = new Button { Text = "Hủy", Left = 335, Top = 95, DialogResult = DialogResult.Cancel };
        dialog.Controls.AddRange([label, input, ok, cancel]);
        dialog.AcceptButton = ok;
        dialog.CancelButton = cancel;
        return dialog.ShowDialog(this) == DialogResult.OK ? input.Text.Trim() : null;
    }
}
