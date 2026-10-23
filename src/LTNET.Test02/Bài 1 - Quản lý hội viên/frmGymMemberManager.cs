using System.ComponentModel;
using System.Globalization;
using LTNET.Test02.Models;
using LTNET.Test02.Validation;

namespace LTNET.Test02;

public sealed class frmGymMemberManager : Form
{
    private readonly IContainer components = new Container();
    private readonly BindingList<MemberModel> members = new();
    private readonly BindingSource memberSource;
    private readonly BindingSource detailSource;
    private readonly ErrorProvider errors;
    private readonly TextBox txtMemberId = new() { Name = "txtMemberId", ReadOnly = true };
    private readonly TextBox txtFullName = new() { Name = "txtFullName" };
    private readonly TextBox txtPhone = new() { Name = "txtPhone" };
    private readonly ComboBox cboPackage = new() { Name = "cboPackage", DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker dtpStartDate = new() { Name = "dtpStartDate", Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy" };
    private readonly TextBox txtFee = new() { Name = "txtFee" };
    private readonly DataGridView dgvMembers = new() { Name = "dgvMembers" };
    private readonly Button btnAdd = new() { Text = "Thêm mới", Name = "btnAdd" };
    private readonly Button btnEdit = new() { Text = "Sửa thông tin", Name = "btnEdit" };
    private readonly Button btnDelete = new() { Text = "Xóa hội viên", Name = "btnDelete" };
    private readonly Button btnReset = new() { Text = "Làm mới", Name = "btnReset" };
    private int nextId = 1;
    private bool isNew;
    private bool changingSelection;

    public frmGymMemberManager()
    {
        memberSource = new BindingSource(components) { DataSource = members };
        detailSource = new BindingSource(components) { DataSource = typeof(MemberModel) };
        errors = new ErrorProvider(components) { ContainerControl = this, BlinkStyle = ErrorBlinkStyle.NeverBlink };
        Name = "frmGymMemberManager";
        Text = "QUẢN LÝ DỊCH VỤ PHÒNG GYM & HỘI VIÊN";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1100, 680);
        MinimumSize = new Size(920, 580);
        Font = new Font("Segoe UI", 10);
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoValidate = AutoValidate.EnableAllowFocusChange;
        BuildLayout();
        ConfigureGrid();
        ConfigureBindings();
        ConfigureValidation();
        memberSource.CurrentChanged += (_, _) => SelectCurrentMember();
        dgvMembers.SelectionChanged += (_, _) => SelectCurrentMember();
        btnAdd.Click += (_, _) => AddMember();
        btnEdit.Click += (_, _) => EditMember();
        btnDelete.Click += (_, _) => DeleteMember();
        btnReset.Click += (_, _) => NewMember();
        NewMember();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 1, RowCount = 4 };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 162));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(new Label { Text = "QUẢN LÝ HỘI VIÊN PHÒNG GYM", AutoSize = true, Font = new Font(Font, FontStyle.Bold), ForeColor = Color.DarkSlateBlue }, 0, 0);
        var fields = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 3 };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        AddField(fields, "Mã hội viên", txtMemberId, 0, 0);
        AddField(fields, "Họ và tên", txtFullName, 0, 1);
        AddField(fields, "Số điện thoại", txtPhone, 0, 2);
        AddField(fields, "Gói tập", cboPackage, 2, 0);
        AddField(fields, "Ngày đăng ký", dtpStartDate, 2, 1);
        AddField(fields, "Phí dịch vụ", txtFee, 2, 2);
        cboPackage.Items.AddRange(["Theo tháng", "6 tháng", "1 năm"]);
        root.Controls.Add(fields, 0, 1);
        var actions = new FlowLayoutPanel { Dock = DockStyle.Fill };
        foreach (var button in new[] { btnAdd, btnEdit, btnDelete, btnReset })
        {
            button.AutoSize = true;
            button.Height = 36;
            button.Margin = new Padding(0, 4, 12, 4);
            button.CausesValidation = false;
            actions.Controls.Add(button);
        }
        root.Controls.Add(actions, 0, 2);
        root.Controls.Add(dgvMembers, 0, 3);
        Controls.Add(root);
    }

    private static void AddField(TableLayoutPanel panel, string caption, Control control, int column, int row)
    {
        panel.Controls.Add(new Label { Text = caption, AutoSize = true, Anchor = AnchorStyles.Left }, column, row);
        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(3, 8, 28, 8);
        panel.Controls.Add(control, column + 1, row);
    }

    private void ConfigureGrid()
    {
        dgvMembers.Dock = DockStyle.Fill;
        dgvMembers.AutoGenerateColumns = false;
        dgvMembers.ReadOnly = true;
        dgvMembers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvMembers.MultiSelect = false;
        dgvMembers.AllowUserToAddRows = false;
        dgvMembers.AllowUserToDeleteRows = false;
        dgvMembers.RowHeadersVisible = false;
        dgvMembers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvMembers.BackgroundColor = Color.White;
        AddColumn("Mã HV", nameof(MemberModel.MemberId), 75);
        AddColumn("Tên hội viên", nameof(MemberModel.FullName), 180);
        AddColumn("SĐT", nameof(MemberModel.Phone), 110);
        AddColumn("Gói tập", nameof(MemberModel.Package), 100);
        AddColumn("Ngày ĐK", nameof(MemberModel.StartDate), 110, "dd/MM/yyyy");
        AddColumn("Phí dịch vụ", nameof(MemberModel.Fee), 115, "N0");
        dgvMembers.Columns[nameof(MemberModel.Fee)]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        dgvMembers.DataSource = memberSource;
    }

    private void AddColumn(string title, string property, int width, string format = "") =>
        dgvMembers.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = property, HeaderText = title, DataPropertyName = property, FillWeight = width,
            SortMode = DataGridViewColumnSortMode.NotSortable,
            DefaultCellStyle = new DataGridViewCellStyle { Format = format, FormatProvider = CultureInfo.GetCultureInfo("vi-VN") }
        });

    private void ConfigureBindings()
    {
        // Read from the selected model immediately; commit control values only after validation.
        Bind(txtMemberId, "Text", nameof(MemberModel.MemberId));
        Bind(txtFullName, "Text", nameof(MemberModel.FullName));
        Bind(txtPhone, "Text", nameof(MemberModel.Phone));
        Bind(cboPackage, "SelectedItem", nameof(MemberModel.Package));
        Bind(dtpStartDate, "Value", nameof(MemberModel.StartDate));
        var feeBinding = Bind(txtFee, "Text", nameof(MemberModel.Fee));
        feeBinding.Format += (_, e) =>
        {
            var fee = Convert.ToInt64(e.Value, CultureInfo.InvariantCulture);
            e.Value = fee > 0 ? fee.ToString(CultureInfo.InvariantCulture) : "";
        };
        feeBinding.Parse += (_, e) => e.Value = long.Parse((string)e.Value!, CultureInfo.InvariantCulture);
    }

    private Binding Bind(Control control, string controlProperty, string modelProperty)
    {
        var binding = new Binding(controlProperty, detailSource, modelProperty, true, DataSourceUpdateMode.Never);
        control.DataBindings.Add(binding);
        return binding;
    }

    private void ConfigureValidation()
    {
        txtFullName.Validating += (_, e) => e.Cancel = SetError(txtFullName, MemberValidator.ValidateFullName(txtFullName.Text));
        txtPhone.Validating += (_, e) => e.Cancel = SetError(txtPhone, MemberValidator.ValidatePhone(txtPhone.Text));
        txtFee.Validating += (_, e) => e.Cancel = SetError(txtFee, MemberValidator.ValidateFee(txtFee.Text));
    }

    private bool SetError(Control control, string message)
    {
        errors.SetError(control, message);
        return message.Length > 0;
    }

    private bool ValidateInput()
    {
        if (ValidateChildren()) return true;
        foreach (var control in new Control[] { txtFullName, txtPhone, txtFee })
            if (errors.GetError(control).Length > 0) { control.Focus(); break; }
        return false;
    }

    private void CommitBindings()
    {
        txtFullName.Text = txtFullName.Text.Trim().Normalize();
        // Suppress detail refresh until every control has written its pending value.
        // The grid's separate BindingSource still receives model change notifications.
        detailSource.RaiseListChangedEvents = false;
        try
        {
            foreach (var control in new Control[] { txtFullName, txtPhone, cboPackage, dtpStartDate, txtFee })
                foreach (Binding binding in control.DataBindings) binding.WriteValue();
            detailSource.EndEdit();
        }
        finally
        {
            detailSource.RaiseListChangedEvents = true;
            detailSource.ResetCurrentItem();
        }
    }

    private void NewMember()
    {
        changingSelection = true;
        try
        {
            dgvMembers.ClearSelection();
            isNew = true;
            detailSource.DataSource = new MemberModel { MemberId = $"HV{nextId:D3}" };
            errors.Clear();
            btnEdit.Enabled = btnDelete.Enabled = false;
        }
        finally { changingSelection = false; }
        txtFullName.Focus();
    }

    private void SelectCurrentMember()
    {
        if (changingSelection || dgvMembers.SelectedRows.Count == 0 || memberSource.Current is not MemberModel member) return;
        isNew = false;
        detailSource.DataSource = member;
        errors.Clear();
        btnEdit.Enabled = btnDelete.Enabled = true;
    }

    private void AddMember()
    {
        // With a row selected, Thêm mới opens an empty entry; click again to save it.
        if (!isNew) { NewMember(); return; }
        if (!ValidateInput()) return;
        CommitBindings();
        var member = (MemberModel)detailSource.Current!;
        members.Add(member);
        nextId++;
        memberSource.Position = members.IndexOf(member);
        dgvMembers.Rows[memberSource.Position].Selected = true;
        SelectCurrentMember();
    }

    private void EditMember()
    {
        if (isNew || !ValidateInput()) return;
        // WriteValue updates the bound instance; INotifyPropertyChanged refreshes its row.
        CommitBindings();
    }

    private void DeleteMember()
    {
        if (isNew || detailSource.Current is not MemberModel member) return;
        if (MessageBox.Show(this, $"Bạn có chắc muốn xóa hội viên {member.FullName} ({member.MemberId})?",
                "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        members.Remove(member);
        if (members.Count == 0) NewMember();
        else
        {
            dgvMembers.Rows[memberSource.Position].Selected = true;
            SelectCurrentMember();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) components.Dispose();
        base.Dispose(disposing);
    }
}
