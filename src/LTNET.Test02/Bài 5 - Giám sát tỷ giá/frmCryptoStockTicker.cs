using System.ComponentModel;
using System.Globalization;

namespace LTNET.Test02.Ticker;

public sealed class frmCryptoStockTicker : Form
{
    private readonly Label lblBTCPrice = new() { Name = "lblBTCPrice", Text = "BTC/USD: 65,000.00", AutoSize = true, Font = new Font("Segoe UI", 24, FontStyle.Bold) };
    private readonly DataGridView dgvHistoryLog = new DataGridView
    {
        Name = "dgvHistoryLog", Dock = DockStyle.Fill, AutoGenerateColumns = false,
        ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
        RowHeadersVisible = false, BackgroundColor = Color.White,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    };
    private readonly BindingList<PriceTick> history = new();
    private readonly ToolStripStatusLabel lblUpdated = new() { Text = "Chưa cập nhật", Spring = true, TextAlign = ContentAlignment.MiddleLeft };
    private readonly ToolStripStatusLabel lblSyncCount = new() { Text = "Đồng bộ thành công: 0" };
    private readonly Label lblState = new() { Text = "Sẵn sàng – giá mô phỏng, không dùng cho giao dịch.", AutoSize = true };
    private readonly Button btnStart, btnPause;
    private CancellationTokenSource? loopCancellation;
    private Task? loopTask;
    private decimal lastPrice = 65_000m;
    private long syncCount;
    private bool closing;

    public frmCryptoStockTicker()
    {
        Text = "Bài 5 – Giám sát BTC/USD thời gian thực (mô phỏng)";
        Size = new Size(1100, 700);
        MinimumSize = new Size(850, 560);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10);
        AutoScaleMode = AutoScaleMode.Dpi;
        Name = "frmCryptoStockTicker";
        btnStart = CreateButton("Bắt đầu Giám sát", "btnStart", (_, _) => StartMonitoring());
        btnPause = CreateButton("Tạm dừng", "btnPause", async (_, _) => await PauseAsync());
        btnPause.Enabled = false;
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 1, RowCount = 4 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 65));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill };
        actions.Controls.AddRange([btnStart, btnPause]);
        root.Controls.Add(actions, 0, 0); root.Controls.Add(lblBTCPrice, 0, 1); root.Controls.Add(lblState, 0, 2);
        dgvHistoryLog.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(PriceTick.Timestamp), DataPropertyName = nameof(PriceTick.Timestamp), HeaderText = "Thời điểm",
            FillWeight = 100, SortMode = DataGridViewColumnSortMode.NotSortable,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "HH:mm:ss", FormatProvider = System.Globalization.CultureInfo.GetCultureInfo("vi-VN")
            }
        });
        dgvHistoryLog.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(PriceTick.PreviousPrice), DataPropertyName = nameof(PriceTick.PreviousPrice), HeaderText = "Giá trước (USD)",
            FillWeight = 100, SortMode = DataGridViewColumnSortMode.NotSortable,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "N2", FormatProvider = System.Globalization.CultureInfo.GetCultureInfo("vi-VN")
            }
        });
        dgvHistoryLog.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(PriceTick.Price), DataPropertyName = nameof(PriceTick.Price), HeaderText = "Giá mới (USD)",
            FillWeight = 100, SortMode = DataGridViewColumnSortMode.NotSortable,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "N2", FormatProvider = System.Globalization.CultureInfo.GetCultureInfo("vi-VN")
            }
        });
        dgvHistoryLog.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(PriceTick.Change), DataPropertyName = nameof(PriceTick.Change), HeaderText = "Thay đổi (USD)",
            FillWeight = 100, SortMode = DataGridViewColumnSortMode.NotSortable,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "+0.00;-0.00;0.00", FormatProvider = System.Globalization.CultureInfo.GetCultureInfo("vi-VN")
            }
        });
        dgvHistoryLog.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = nameof(PriceTick.Direction), DataPropertyName = nameof(PriceTick.Direction), HeaderText = "Xu hướng",
            FillWeight = 100, SortMode = DataGridViewColumnSortMode.NotSortable,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "", FormatProvider = System.Globalization.CultureInfo.GetCultureInfo("vi-VN")
            }
        });
        dgvHistoryLog.DataSource = history;
        root.Controls.Add(dgvHistoryLog, 0, 3);
        var status = new StatusStrip(); status.Items.AddRange([lblUpdated, lblSyncCount]);
        Controls.Add(root); Controls.Add(status);
        FormClosing += OnFormClosing;
    }

    private void StartMonitoring()
    {
        if (loopTask is not null || closing) return;
        loopCancellation = new CancellationTokenSource();
        var token = loopCancellation.Token;
        decimal initialPrice = lastPrice; // Read state on the UI thread before starting the worker.
        btnStart.Enabled = false; btnPause.Enabled = true;
        lblState.Text = "Đang giám sát – cập nhật mỗi 2 giây (mô phỏng).";
        loopTask = Task.Run(async () =>
        {
            decimal price = initialPrice;
            var random = new Random();
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                var tick = PriceSimulator.Next(price, DateTime.Now, random);
                price = tick.Price;
                PublishTick(tick, token);
            }
        }, token);
        // Observe unexpected failures even if the user never presses Pause.
        _ = ObserveLoopAsync(loopTask);
    }

    private void PublishTick(PriceTick tick, CancellationToken token)
    {
        if (token.IsCancellationRequested) return;
        // This method is called by the background task: controls are only changed via Invoke.
        if (InvokeRequired)
        {
            try
            {
                Invoke(new Action(() =>
                {
                    if (!closing && !IsDisposed && !token.IsCancellationRequested) ApplyTick(tick);
                }));
            }
            catch (ObjectDisposedException) when (token.IsCancellationRequested) { }
            catch (InvalidOperationException) when (token.IsCancellationRequested) { }
            return;
        }
        if (!closing && !IsDisposed && !token.IsCancellationRequested) ApplyTick(tick);
    }

    private void ApplyTick(PriceTick tick)
    {
        lastPrice = tick.Price;
        lblBTCPrice.Text = "BTC/USD: " + tick.Price.ToString("N2", CultureInfo.GetCultureInfo("en-US"));
        lblBTCPrice.ForeColor = tick.Change > 0 ? Color.ForestGreen : tick.Change < 0 ? Color.Firebrick : SystemColors.ControlText;
        history.Insert(0, tick);
        // Bound the UI log for long sessions; syncCount still counts every successful update.
        if (history.Count > 1000) history.RemoveAt(history.Count - 1);
        syncCount++;
        lblUpdated.Text = $"Lần cập nhật cuối: {tick.Timestamp:HH:mm:ss}";
        lblSyncCount.Text = $"Đồng bộ thành công: {syncCount:N0}";
    }

    private async Task ObserveLoopAsync(Task task)
    {
        try { await task; }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            if (!closing) lblState.Text = "Giám sát thất bại: " + ex.Message;
        }
        finally
        {
            // Continuation resumes on the UI thread. Never block it with Wait()/Result,
            // as the worker may be waiting in Invoke during shutdown.
            if (ReferenceEquals(loopTask, task))
            {
                loopTask = null;
                loopCancellation?.Dispose(); loopCancellation = null;
                if (!closing) { btnStart.Enabled = true; btnPause.Enabled = false; }
            }
        }
    }

    private async Task PauseAsync()
    {
        var task = loopTask;
        if (task is null) return;
        btnPause.Enabled = false;
        loopCancellation!.Cancel();
        lblState.Text = "Đang dừng...";
        try { await task; }
        catch (OperationCanceledException) { }
        catch (Exception) { /* ObserveLoopAsync reports the failure. */ }
        if (!closing) lblState.Text = "Đã tạm dừng. Có thể tiếp tục giám sát.";
    }

    private async void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (loopTask is null) return;
        e.Cancel = true;
        if (closing) return;
        closing = true;
        btnStart.Enabled = btnPause.Enabled = false;
        loopCancellation!.Cancel();
        var task = loopTask;
        try { await task; }
        catch (OperationCanceledException) { }
        catch (Exception) { }
        // The worker has exited, so no Invoke can race with disposal of the window handle.
        FormClosing -= OnFormClosing;
        Close();
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
