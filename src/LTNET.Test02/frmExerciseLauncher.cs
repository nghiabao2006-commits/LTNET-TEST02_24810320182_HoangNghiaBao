using LTNET.Test02.Analytics;
using LTNET.Test02.Books;
using LTNET.Test02.Hospital;
using LTNET.Test02.Ticker;

namespace LTNET.Test02;

public sealed class frmExerciseLauncher : Form
{
    public frmExerciseLauncher()
    {
        Text = "LTNET-TEST02 – 5 bài thực hành Windows Forms";
        Size = new Size(930, 600);
        MinimumSize = new Size(850, 560);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10);
        AutoScaleMode = AutoScaleMode.Dpi;
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(24), FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
        panel.Controls.Add(new Label { Text = "CHỌN BÀI THỰC HÀNH", Font = new Font(Font, FontStyle.Bold), AutoSize = true, Margin = new Padding(6, 6, 6, 20) });
        AddExercise(panel, "Bài 1 – Quản lý hội viên phòng gym", () => new frmGymMemberManager());
        AddExercise(panel, "Bài 2 – Quản lý sách, ảnh bìa & CSV", () => new frmBookManager());
        AddExercise(panel, "Bài 3 – Hệ thống bệnh viện MDI", () => new frmHospitalMain());
        AddExercise(panel, "Bài 4 – Thống kê doanh thu bất đồng bộ", () => new frmRevenueAnalytics());
        AddExercise(panel, "Bài 5 – Giám sát tỷ giá BTC/USD mô phỏng", () => new frmCryptoStockTicker());
        panel.Controls.Add(new Label { Text = "Dữ liệu mẫu được lưu trong bộ nhớ. Bài 2 hỗ trợ lưu/mở CSV.\nBài 4 và 5 sử dụng dữ liệu mô phỏng, không cần dịch vụ ngoài.", AutoSize = true, Margin = new Padding(6, 20, 6, 6) });
        Controls.Add(panel);
    }

    private void AddExercise(FlowLayoutPanel panel, string text, Func<Form> create)
    {
        var button = CreateButton(text, "btnExercise" + panel.Controls.Count, (_, _) =>
        {
            using var form = create();
            Hide();
            try { form.ShowDialog(); }
            finally { Show(); }
        });
        button.Width = 720;
        button.Height = 52;
        panel.Controls.Add(button);
    }

    // Hàm tạo nút chỉ được dùng trong form này.
    private static Button CreateButton(string text, string name, EventHandler click)
    {
        var button = new Button
        {
            Text = text, Name = name, AutoSize = true, Height = 36,
            Margin = new Padding(5), CausesValidation = false
        };
        button.Click += click;
        return button;
    }
}
