using System.ComponentModel;
using System.Text.RegularExpressions;

namespace LTNET.Test02.Hospital;

public sealed record EditorField(string Property, string Caption, string[]? Choices = null, bool IsPhone = false, bool IsBirthDate = false);

// Shared editor for the three independent MDI child windows; lists belong to the parent.
public class RecordEditorForm<T> : Form where T : class, new()
{
    private readonly IContainer components = new Container();
    private readonly BindingList<T> records;
    private readonly BindingSource source;
    private readonly ErrorProvider errors;
    private readonly string idProperty, prefix;
    private readonly EditorField[] fields;
    private readonly Dictionary<string, Control> inputs = new();
    private readonly DataGridView grid = new DataGridView
    {
        Name = "dgvRecords", Dock = DockStyle.Fill, AutoGenerateColumns = false,
        ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
        RowHeadersVisible = false, BackgroundColor = Color.White,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    };
    private readonly TextBox idInput = new() { ReadOnly = true };
    private T? editing;
    private int nextId = 1;

    public RecordEditorForm(string title, BindingList<T> records, string idProperty, string prefix, params EditorField[] fields)
    {
        this.records = records; this.idProperty = idProperty; this.prefix = prefix; this.fields = fields;
        Text = title;
        Size = new Size(1000, 650);
        MinimumSize = new Size(850, 560);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10);
        AutoScaleMode = AutoScaleMode.Dpi;
        source = new BindingSource(components) { DataSource = records };
        errors = new ErrorProvider(components) { ContainerControl = this, BlinkStyle = ErrorBlinkStyle.NeverBlink };
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10), ColumnCount = 2 };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        var detail = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = fields.Length + 3 };
        detail.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108));
        detail.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        detail.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        AddField(detail, "Mã hồ sơ", idInput, 0);
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = idProperty, DataPropertyName = idProperty, HeaderText = "Mã",
            FillWeight = 100, SortMode = DataGridViewColumnSortMode.NotSortable,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Format = "", FormatProvider = System.Globalization.CultureInfo.GetCultureInfo("vi-VN")
            }
        });
        for (int i = 0; i < fields.Length; i++)
        {
            var field = fields[i];
            var property = typeof(T).GetProperty(field.Property)!;
            Control input;
            if (property.PropertyType == typeof(DateTime))
                input = new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", MaxDate = field.IsBirthDate ? DateTime.Today : DateTimePicker.MaximumDateTime };
            else if (field.Choices is not null)
            {
                var combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
                combo.Items.AddRange(field.Choices.Cast<object>().ToArray());
                combo.SelectedIndex = 0;
                input = combo;
            }
            else input = new TextBox();
            input.Name = "input" + field.Property;
            inputs.Add(field.Property, input);
            detail.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            AddField(detail, field.Caption, input, i + 1);
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = field.Property, DataPropertyName = field.Property, HeaderText = field.Caption,
                FillWeight = 100, SortMode = DataGridViewColumnSortMode.NotSortable,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = property.PropertyType == typeof(DateTime) ? "dd/MM/yyyy" : "", FormatProvider = System.Globalization.CultureInfo.GetCultureInfo("vi-VN")
                }
            });
        }
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        actions.Controls.Add(CreateButton("Thêm mới", "btnNew", (_, _) => NewRecord()));
        actions.Controls.Add(CreateButton("Lưu", "btnSave", (_, _) => SaveRecord()));
        actions.Controls.Add(CreateButton("Xóa", "btnDelete", (_, _) => DeleteRecord()));
        detail.Controls.Add(actions, 0, fields.Length + 1); detail.SetColumnSpan(actions, 2);
        detail.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        if (typeof(T) == typeof(PatientModel))
        {
            var view = CreateButton("Truy xuất hồ sơ bệnh án", "btnViewProfile", (_, _) => ViewPatient());
            detail.Controls.Add(view, 0, fields.Length + 2); detail.SetColumnSpan(view, 2);
        }
        root.Controls.Add(detail, 0, 0); root.Controls.Add(grid, 1, 0);
        Controls.Add(root);
        grid.DataSource = source;
        source.CurrentChanged += (_, _) => ShowCurrent();
        grid.SelectionChanged += (_, _) => { if (grid.SelectedRows.Count > 0) ShowCurrent(); };
        if (records.Count > 0) ShowCurrent(); else NewRecord();
    }

    private void NewRecord()
    {
        editing = null;
        while (records.Any(r => string.Equals((string?)typeof(T).GetProperty(idProperty)!.GetValue(r), $"{prefix}{nextId:D3}", StringComparison.OrdinalIgnoreCase))) nextId++;
        idInput.Text = $"{prefix}{nextId:D3}";
        foreach (var input in inputs.Values)
        {
            if (input is DateTimePicker date) date.Value = DateTime.Today;
            else if (input is ComboBox combo) combo.SelectedIndex = 0;
            else input.Text = "";
        }
        grid.ClearSelection(); errors.Clear();
    }

    private void ShowCurrent()
    {
        if (source.Current is not T record) { editing = null; return; }
        editing = record;
        idInput.Text = (string?)typeof(T).GetProperty(idProperty)!.GetValue(record);
        foreach (var field in fields)
        {
            var value = typeof(T).GetProperty(field.Property)!.GetValue(record);
            if (inputs[field.Property] is DateTimePicker date) date.Value = (DateTime)value!;
            else inputs[field.Property].Text = (string?)value ?? "";
        }
        errors.Clear();
    }

    private void SaveRecord()
    {
        errors.Clear();
        bool valid = true;
        foreach (var field in fields)
        {
            var input = inputs[field.Property];
            string error = string.IsNullOrWhiteSpace(input.Text) ? "Thông tin không được để trống." : "";
            if (field.IsPhone)
                error = Regex.IsMatch(input.Text, @"\A[0-9]{10}\z")
                    ? "" : "Số điện thoại phải gồm đúng 10 chữ số (0–9).";
            if (error.Length > 0) { errors.SetError(input, error); valid = false; }
        }
        if (!valid) return;
        var record = editing ?? new T();
        typeof(T).GetProperty(idProperty)!.SetValue(record, idInput.Text);
        foreach (var field in fields)
        {
            var input = inputs[field.Property];
            object value = input is DateTimePicker date ? date.Value.Date : input.Text.Trim();
            typeof(T).GetProperty(field.Property)!.SetValue(record, value);
        }
        if (editing is null) { records.Add(record); nextId++; }
        source.Position = records.IndexOf(record);
        source.ResetCurrentItem();
        grid.Rows[source.Position].Selected = true;
        ShowCurrent();
    }

    private void DeleteRecord()
    {
        if (editing is null) return;
        if (MessageBox.Show(this, "Xóa hồ sơ đang chọn?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        records.Remove(editing);
        if (records.Count == 0) NewRecord(); else ShowCurrent();
    }

    private void ViewPatient()
    {
        if (editing is not PatientModel patient) { MessageBox.Show(this, "Chọn một bệnh nhân đã lưu.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        MessageBox.Show(this, $"Mã: {patient.PatientId}\nHọ tên: {patient.FullName}\nNgày sinh: {patient.BirthDate:dd/MM/yyyy}\nGiới tính: {patient.Gender}\nĐiện thoại: {patient.Phone}\nKhoa: {patient.Department}\n\nBệnh án:\n{patient.Diagnosis}", "Hồ sơ bệnh án", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) components.Dispose();
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
}
