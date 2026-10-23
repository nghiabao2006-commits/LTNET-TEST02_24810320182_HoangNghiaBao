using System.ComponentModel;

namespace LTNET.Test02.Books;

public sealed class frmBookManager : Form
{
    private readonly IContainer components = new Container();
    private readonly BindingList<BookModel> books = new();
    private readonly BindingSource source;
    private readonly ErrorProvider errors;
    private readonly TextBox txtBookId = new() { Name = "txtBookId", ReadOnly = true };
    private readonly TextBox txtTitle = new() { Name = "txtTitle" };
    private readonly TextBox txtAuthor = new() { Name = "txtAuthor" };
    private readonly NumericUpDown nudPrice = new() { Name = "nudPrice", Maximum = 1_000_000_000, DecimalPlaces = 2, ThousandsSeparator = true };
    private readonly NumericUpDown nudQuantity = new() { Name = "nudQuantity", Maximum = 1_000_000 };
    private readonly PictureBox picBookCover = new() { Name = "picBookCover", Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BorderStyle = BorderStyle.FixedSingle };
    private readonly Label lblCover = new() { AutoSize = true, MaximumSize = new Size(330, 0) };
    private readonly DataGridView dgvBooks = new DataGridView
    {
        Name = "dgvBooks", Dock = DockStyle.Fill, AutoGenerateColumns = false,
        ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
        RowHeadersVisible = false, BackgroundColor = Color.White,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    };
    private readonly ToolStripStatusLabel lblCount = new(), lblValue = new(), lblClock = new();
    private readonly System.Windows.Forms.Timer clock;
    private BookModel? editing;
    private string coverPath = "";
    private bool dirty;
    private int nextId = 1;

    public frmBookManager()
    {
        Text = "Bài 2 – Quản lý sách & báo cáo CSV";
        Size = new Size(1200, 780);
        MinimumSize = new Size(850, 560);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10);
        AutoScaleMode = AutoScaleMode.Dpi;
        source = new BindingSource(components) { DataSource = books };
        errors = new ErrorProvider(components) { ContainerControl = this, BlinkStyle = ErrorBlinkStyle.NeverBlink };
        BuildLayout();
        foreach (var (property, title, format) in new[]
        {
            (nameof(BookModel.BookId), "Mã sách", ""), (nameof(BookModel.Title), "Tên sách", ""),
            (nameof(BookModel.Author), "Tác giả", ""), (nameof(BookModel.Price), "Đơn giá", "N0"),
            (nameof(BookModel.Quantity), "Số lượng", "N0"), (nameof(BookModel.CoverFileName), "Ảnh bìa", "")
        })
        {
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = property, DataPropertyName = property, HeaderText = title,
                FillWeight = 100, SortMode = DataGridViewColumnSortMode.NotSortable,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = format, FormatProvider = System.Globalization.CultureInfo.GetCultureInfo("vi-VN")
                }
            });
        }
        dgvBooks.DataSource = source;
        source.CurrentChanged += (_, _) => ShowCurrent();
        dgvBooks.SelectionChanged += (_, _) => { if (dgvBooks.SelectedRows.Count > 0) ShowCurrent(); };
        books.ListChanged += (_, _) => { dirty = true; UpdateTotals(); };
        clock = new System.Windows.Forms.Timer(components) { Interval = 1000 };
        clock.Tick += (_, _) => lblClock.Text = DateTime.Now.ToString("HH:mm:ss");
        clock.Start();
        lblClock.Text = DateTime.Now.ToString("HH:mm:ss");
        UpdateTotals();
        NewBook();
        FormClosing += (_, e) => e.Cancel = !ConfirmDiscard();
    }

    private void BuildLayout()
    {
        var menu = new MenuStrip();
        var file = new ToolStripMenuItem("&File");
        file.DropDownItems.Add("Mở file CSV", null, (_, _) => OpenCsv());
        file.DropDownItems.Add("Lưu file CSV", null, (_, _) => SaveCsv());
        file.DropDownItems.Add("Thoát", null, (_, _) => Close());
        var edit = new ToolStripMenuItem("&Edit");
        edit.DropDownItems.Add("Thêm", null, (_, _) => NewBook());
        edit.DropDownItems.Add("Xóa", null, (_, _) => DeleteBook());
        edit.DropDownItems.Add("Tìm kiếm", null, (_, _) => Search());
        menu.Items.AddRange([file, edit]);
        MainMenuStrip = menu;
        var tools = new ToolStrip();
        tools.Items.Add(new ToolStripButton("Mở file", SystemIcons.Application.ToBitmap(), (_, _) => OpenCsv()));
        tools.Items.Add(new ToolStripButton("Lưu file", SystemIcons.Shield.ToBitmap(), (_, _) => SaveCsv()));
        tools.Items.Add(new ToolStripButton("Thêm mới", SystemIcons.Information.ToBitmap(), (_, _) => NewBook()));
        var status = new StatusStrip();
        lblValue.Spring = true;
        status.Items.AddRange([lblCount, lblValue, lblClock]);
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(12) };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 380));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var details = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 10 };
        details.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        details.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 5; i++) details.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        details.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        details.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        details.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        details.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        details.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        AddField(details, "Mã sách", txtBookId, 0);
        AddField(details, "Tên sách", txtTitle, 1);
        AddField(details, "Tác giả", txtAuthor, 2);
        AddField(details, "Đơn giá", nudPrice, 3);
        AddField(details, "Số lượng", nudQuantity, 4);
        var upload = CreateButton("Tải ảnh bìa", "btnLoadCover", (_, _) => LoadCover());
        details.Controls.Add(upload, 0, 5); details.SetColumnSpan(upload, 2);
        details.Controls.Add(picBookCover, 0, 6); details.SetColumnSpan(picBookCover, 2);
        details.Controls.Add(lblCover, 0, 7); details.SetColumnSpan(lblCover, 2);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill };
        actions.Controls.Add(CreateButton("Lưu thông tin", "btnSaveBook", (_, _) => SaveBook()));
        actions.Controls.Add(CreateButton("Xóa", "btnDeleteBook", (_, _) => DeleteBook()));
        actions.Controls.Add(CreateButton("Làm mới", "btnNewBook", (_, _) => NewBook()));
        details.Controls.Add(actions, 0, 8); details.SetColumnSpan(actions, 2);
        var hint = new Label { Text = "Chọn dòng để sửa; lưu CSV để giữ dữ liệu.", AutoSize = true };
        details.Controls.Add(hint, 0, 9); details.SetColumnSpan(hint, 2);
        root.Controls.Add(details, 0, 0); root.Controls.Add(dgvBooks, 1, 0);
        Controls.Add(root); Controls.Add(status); Controls.Add(tools); Controls.Add(menu);
    }

    private void NewBook()
    {
        editing = null;
        while (books.Any(b => b.BookId.Equals($"S{nextId:D3}", StringComparison.OrdinalIgnoreCase))) nextId++;
        txtBookId.Text = $"S{nextId:D3}";
        txtTitle.Clear(); txtAuthor.Clear(); nudPrice.Value = 0; nudQuantity.Value = 0;
        errors.Clear(); coverPath = ""; ShowCover();
        dgvBooks.ClearSelection();
        txtTitle.Focus();
    }

    private void ShowCurrent()
    {
        if (source.Current is not BookModel book) return;
        editing = book;
        txtBookId.Text = book.BookId; txtTitle.Text = book.Title; txtAuthor.Text = book.Author;
        nudPrice.Value = book.Price; nudQuantity.Value = book.Quantity;
        coverPath = book.CoverPath; errors.Clear(); ShowCover();
    }

    private void SaveBook()
    {
        errors.Clear();
        if (string.IsNullOrWhiteSpace(txtTitle.Text)) errors.SetError(txtTitle, "Nhập tên sách.");
        if (string.IsNullOrWhiteSpace(txtAuthor.Text)) errors.SetError(txtAuthor, "Nhập tác giả.");
        if (nudPrice.Value <= 0) errors.SetError(nudPrice, "Đơn giá phải lớn hơn 0.");
        if (new Control[] { txtTitle, txtAuthor, nudPrice }.Any(c => errors.GetError(c).Length > 0)) return;
        var book = editing ?? new BookModel { BookId = txtBookId.Text };
        book.Title = txtTitle.Text.Trim(); book.Author = txtAuthor.Text.Trim();
        book.Price = nudPrice.Value; book.Quantity = (int)nudQuantity.Value; book.CoverPath = coverPath;
        if (editing is null) { books.Add(book); nextId++; }
        source.Position = books.IndexOf(book);
        dgvBooks.Rows[source.Position].Selected = true;
        ShowCurrent();
    }

    private void DeleteBook()
    {
        if (editing is null) return;
        if (MessageBox.Show(this, $"Xóa sách {editing.Title}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        books.Remove(editing);
        if (books.Count == 0) NewBook(); else ShowCurrent();
    }

    private void LoadCover()
    {
        using var dialog = new OpenFileDialog { Filter = "Ảnh bìa (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg", CheckFileExists = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            using var image = Image.FromFile(dialog.FileName);
            SetImage(new Bitmap(image)); // Release file handle; never keep the source image locked.
            coverPath = Path.GetFullPath(dialog.FileName);
            lblCover.Text = Path.GetFileName(coverPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or OutOfMemoryException)
        { MessageBox.Show(this, "Không đọc được ảnh: " + ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }

    private void ShowCover()
    {
        SetImage(null);
        lblCover.Text = Path.GetFileName(coverPath);
        if (coverPath.Length == 0) return;
        try { using var image = Image.FromFile(coverPath); SetImage(new Bitmap(image)); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or OutOfMemoryException)
        { lblCover.Text = "Không đọc được ảnh: " + Path.GetFileName(coverPath); }
    }

    private void SetImage(Image? image)
    {
        var old = picBookCover.Image;
        picBookCover.Image = image;
        old?.Dispose();
    }

    private void OpenCsv()
    {
        if (!ConfirmDiscard()) return;
        using var dialog = new OpenFileDialog { Filter = "CSV (*.csv)|*.csv", CheckFileExists = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var loaded = BookCsv.Load(dialog.FileName); // Parse everything before replacing current data.
            books.RaiseListChangedEvents = false;
            try { books.Clear(); foreach (var book in loaded) books.Add(book); }
            finally { books.RaiseListChangedEvents = true; books.ResetBindings(); }
            dirty = false; nextId = 1;
            if (books.Count == 0) NewBook(); else { source.Position = 0; ShowCurrent(); }
            UpdateTotals();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException or ArgumentException or NotSupportedException)
        { MessageBox.Show(this, "Không mở được CSV; dữ liệu hiện tại được giữ lại.\n" + ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }

    private bool SaveCsv()
    {
        using var dialog = new SaveFileDialog { Filter = "CSV (*.csv)|*.csv", DefaultExt = "csv", AddExtension = true, FileName = "BaoCaoSach.csv" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return false;
        try { BookCsv.Save(dialog.FileName, books); dirty = false; return true; }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException or ArgumentException or NotSupportedException)
        { MessageBox.Show(this, "Không lưu được CSV: " + ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false; }
    }

    private bool ConfirmDiscard()
    {
        if (!dirty) return true;
        var result = MessageBox.Show(this, "Lưu danh sách sách vào CSV trước khi tiếp tục?", "Dữ liệu chưa lưu", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
        return result == DialogResult.No || result == DialogResult.Yes && SaveCsv();
    }

    private void Search()
    {
        var text = ShowInputDialog("Tìm kiếm sách", "Nhập mã, tên sách hoặc tác giả:");
        if (string.IsNullOrEmpty(text)) return;
        var match = books.FirstOrDefault(b => b.BookId.Contains(text, StringComparison.CurrentCultureIgnoreCase) || b.Title.Contains(text, StringComparison.CurrentCultureIgnoreCase) || b.Author.Contains(text, StringComparison.CurrentCultureIgnoreCase));
        if (match is null) { MessageBox.Show(this, "Không tìm thấy sách.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        source.Position = books.IndexOf(match);
        dgvBooks.Rows[source.Position].Selected = true;
        dgvBooks.FirstDisplayedScrollingRowIndex = source.Position;
        ShowCurrent();
    }

    private void UpdateTotals()
    {
        lblCount.Text = $"Tổng đầu sách: {books.Count:N0} | Số cuốn: {books.Sum(b => (long)b.Quantity):N0}";
        lblValue.Text = $"Tổng giá trị kho: {books.Sum(b => b.InventoryValue):N0} đ";
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { SetImage(null); components.Dispose(); }
        base.Dispose(disposing);
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

    private static void AddField(TableLayoutPanel panel, string label, Control control, int row)
    {
        panel.Controls.Add(new Label
        {
            Text = label, AutoSize = true, Anchor = AnchorStyles.Left
        }, 0, row);
        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(4, 5, 24, 5);
        panel.Controls.Add(control, 1, row);
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
