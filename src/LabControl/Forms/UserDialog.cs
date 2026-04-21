using LabControl.Models;

namespace LabControl.Forms;

/// <summary>Dialog for adding or editing a LabUser.</summary>
public class UserDialog : Form
{
    private readonly TextBox _txtStudentId = new();
    private readonly TextBox _txtFirstName = new();
    private readonly TextBox _txtLastName = new();
    private readonly TextBox _txtDepartment = new();
    private readonly TextBox _txtEmail = new();
    private readonly CheckBox _chkActive = new();
    private readonly Button _btnOk = new();
    private readonly Button _btnCancel = new();

    public LabUser User { get; private set; }

    public UserDialog(LabUser? user = null)
    {
        User = user ?? new LabUser();
        InitializeLayout();
        if (user != null) PopulateFields();
    }

    private void InitializeLayout()
    {
        Text = User.Id == 0 ? "Add User" : "Edit User";
        Size = new Size(400, 320);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 2,
            RowCount = 8
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        void AddRow(int row, string label, Control ctrl)
        {
            table.Controls.Add(new Label { Text = label, TextAlign = ContentAlignment.MiddleLeft, AutoSize = false, Dock = DockStyle.Fill }, 0, row);
            ctrl.Dock = DockStyle.Fill;
            table.Controls.Add(ctrl, 1, row);
        }

        _chkActive.Text = "Active";
        _chkActive.Checked = true;

        AddRow(0, "Student ID *", _txtStudentId);
        AddRow(1, "First Name *", _txtFirstName);
        AddRow(2, "Last Name *", _txtLastName);
        AddRow(3, "Department", _txtDepartment);
        AddRow(4, "Email", _txtEmail);
        AddRow(5, "Status", _chkActive);

        _btnOk.Text = "OK";
        _btnOk.DialogResult = DialogResult.OK;
        _btnOk.Click += BtnOk_Click;

        _btnCancel.Text = "Cancel";
        _btnCancel.DialogResult = DialogResult.Cancel;

        var btnPanel = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 4, 0, 0)
        };
        btnPanel.Controls.AddRange(new Control[] { _btnCancel, _btnOk });
        table.Controls.Add(btnPanel, 0, 7);
        table.SetColumnSpan(btnPanel, 2);

        Controls.Add(table);
        AcceptButton = _btnOk;
        CancelButton = _btnCancel;
    }

    private void PopulateFields()
    {
        _txtStudentId.Text = User.StudentId;
        _txtFirstName.Text = User.FirstName;
        _txtLastName.Text = User.LastName;
        _txtDepartment.Text = User.Department;
        _txtEmail.Text = User.Email;
        _chkActive.Checked = User.IsActive;
    }

    private void BtnOk_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtStudentId.Text) ||
            string.IsNullOrWhiteSpace(_txtFirstName.Text) ||
            string.IsNullOrWhiteSpace(_txtLastName.Text))
        {
            MessageBox.Show("Student ID, First Name, and Last Name are required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
            return;
        }

        User.StudentId = _txtStudentId.Text.Trim();
        User.FirstName = _txtFirstName.Text.Trim();
        User.LastName = _txtLastName.Text.Trim();
        User.Department = _txtDepartment.Text.Trim();
        User.Email = _txtEmail.Text.Trim();
        User.IsActive = _chkActive.Checked;
    }
}
