using System.ComponentModel;

namespace LTNET.Test02.Analytics;

public sealed class frmRevenueAnalytics : Form
{
    private readonly DateTimePicker dtpFrom = new() { Name = "dtpFromDate", Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-30) };
    private readonly DateTimePicker dtpTo = new() { Name = "dtpToDate", Format = DateTimePickerFormat.Short, Value = DateTime.Today };
    private readonly ComboBox cboBranch = new() { Name = "cboBranch", DropDownStyle = ComboBoxStyle.DropDownList, Width = 160 };
    private readonly ProgressBar pgbLoading = new() { Name = "pgbLoading", Dock = DockStyle.Fill, Minimum = 0, Maximum = 100 };
    private readonly Label lblProcessingStatus = new() { Name = "lblProcessingStatus", Text = "Sẵn sàng. Dữ liệu thống kê là dữ liệu mô phỏng.", AutoSize = true };
    private readonly DataGridView dgvRevenueData = new DataGridView
    {
        Name = "dgvRevenueData", Dock = DockStyle.Fill, AutoGenerateColumns = false,
        ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
        RowHeadersVisible = false, BackgroundColor = Color.White,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    };
    private readonly Button btnStart, btnCancel;
    private CancellationTokenSource? activeCancellation;
    private bool closeRequested;

    public frmRevenueAnalytics()
    {
        Text = "Bài 4 – Thống kê doanh thu async/await";
        Size = new Size(1100, 700);
        MinimumSize = new Size(850, 560);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10);
        AutoScaleMode = AutoScaleMode.Dpi;
        Name = "frmRevenueAnalytics";
        btnStart = CreateButton("Thống Kê Doanh Thu", "btnAnalyze", async (_, _) => await AnalyzeAsync());
        btnCancel = CreateButton("Hủy Thao Tác", "btnCancel", (sender, _) =>
        {
            activeCancellation?.Cancel();
            ((Button)sender!).Enabled = false;
            lblProcessingStatus.Text = "Đang hủy thao tác...";
        });
        btnCancel.Enabled = false;
        cboBranch.Items.AddRange(RevenueService.Branches);
        cboBranch.SelectedIndex = 0;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 1, RowCount = 5 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var criteria = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true };
        criteria.Controls.AddRange([new Label { Text = "Từ ngày", AutoSize = true }, dtpFrom, new Label { Text = "Đến ngày", AutoSize = true }, dtpTo, cboBranch, btnStart, btnCancel]);
        var notes = new FlowLayoutPanel { Dock = DockStyle.Fill };
        notes.Controls.Add(new Label { Text = "Ghi chú (vẫn nhập được khi đang xử lý):", AutoSize = true });
        notes.Controls.Add(new TextBox { Name = "txtNotes", Width = 450 });
        root.Controls.Add(criteria, 0, 0); root.Controls.Add(notes, 0, 1);
        root.Controls.Add(pgbLoading, 0, 2); root.Controls.Add(lblProcessingStatus, 0, 3);
        dgvRevenueData.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(RevenueTransaction.TransactionId), DataPropertyName = nameof(RevenueTransaction.TransactionId), HeaderText = "Mã giao dịch",
            FillWeight = 100, SortMode = DataGridViewColumnSortMode.NotSortable,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "", FormatProvider = System.Globalization.CultureInfo.GetCultureInfo("vi-VN")
            }
        });
        dgvRevenueData.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(RevenueTransaction.OccurredAt), DataPropertyName = nameof(RevenueTransaction.OccurredAt), HeaderText = "Thời điểm",
            FillWeight = 100, SortMode = DataGridViewColumnSortMode.NotSortable,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "dd/MM/yyyy HH:mm", FormatProvider = System.Globalization.CultureInfo.GetCultureInfo("vi-VN")
            }
        });
        dgvRevenueData.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(RevenueTransaction.Branch), DataPropertyName = nameof(RevenueTransaction.Branch), HeaderText = "Chi nhánh",
            FillWeight = 100, SortMode = DataGridViewColumnSortMode.NotSortable,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "", FormatProvider = System.Globalization.CultureInfo.GetCultureInfo("vi-VN")
            }
        });
        dgvRevenueData.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(RevenueTransaction.Amount), DataPropertyName = nameof(RevenueTransaction.Amount), HeaderText = "Doanh thu (đ)",
            FillWeight = 100, SortMode = DataGridViewColumnSortMode.NotSortable,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "N0", FormatProvider = System.Globalization.CultureInfo.GetCultureInfo("vi-VN")
            }
        });
        root.Controls.Add(dgvRevenueData, 0, 4); Controls.Add(root);
        FormClosing += (_, e) =>
        {
            if (activeCancellation is null) return;
            e.Cancel = true; closeRequested = true;
            activeCancellation.Cancel();
            lblProcessingStatus.Text = "Đang dừng tác vụ trước khi đóng...";
        };
    }

    private async Task AnalyzeAsync()
    {
        if (activeCancellation is not null) return;
        // Snapshot criteria on the UI thread; later user edits apply to the next request.
        var request = new RevenueRequest(dtpFrom.Value.Date, dtpTo.Value.Date, (string)cboBranch.SelectedItem!);
        try { RevenueService.Validate(request); }
        catch (ArgumentException ex) { MessageBox.Show(this, ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        using var cancellation = new CancellationTokenSource();
        activeCancellation = cancellation;
        btnStart.Enabled = false; btnCancel.Enabled = true;
        pgbLoading.Value = 0;
        lblProcessingStatus.Text = "Đang truy xuất dữ liệu mô phỏng...";
        var progress = new Progress<RevenueProgress>(update =>
        {
            if (!ReferenceEquals(activeCancellation, cancellation) || IsDisposed || closeRequested) return;
            pgbLoading.Value = update.Percent;
            lblProcessingStatus.Text = update.Message;
        });
        try
        {
            // Task.Run unwraps the async worker; Progress<T> marshals updates to the UI context.
            var result = await Task.Run(() => RevenueService.GenerateAsync(request, progress, cancellation.Token), cancellation.Token);
            cancellation.Token.ThrowIfCancellationRequested();
            dgvRevenueData.DataSource = new BindingList<RevenueTransaction>(result);
            pgbLoading.Value = 100;
            lblProcessingStatus.Text = $"Hoàn tất: {result.Count:N0} giao dịch | Tổng doanh thu: {result.Sum(r => r.Amount):N0} đ (mô phỏng)";
        }
        catch (OperationCanceledException)
        {
            pgbLoading.Value = 0;
            lblProcessingStatus.Text = "Đã hủy thao tác!";
        }
        catch (Exception ex)
        {
            pgbLoading.Value = 0;
            lblProcessingStatus.Text = "Thống kê thất bại.";
            if (!closeRequested) MessageBox.Show(this, ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally
        {
            activeCancellation = null;
            btnStart.Enabled = true; btnCancel.Enabled = false;
            if (closeRequested) Close();
        }
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
