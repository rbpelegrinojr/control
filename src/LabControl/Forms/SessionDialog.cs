using LabControl.Models;
using LabControl.Services;

namespace LabControl.Forms;

/// <summary>Dialog for starting a new lab session.</summary>
public class SessionDialog : Form
{
    private readonly ComboBox _cboStation = new();
    private readonly ComboBox _cboUser = new();
    private readonly TextBox _txtPurpose = new();
    private readonly TextBox _txtNotes = new();
    private readonly Button _btnOk = new();
    private readonly Button _btnCancel = new();

    private readonly List<Station> _stations;
    private readonly List<LabUser> _users;

    public Session Session { get; private set; } = new();

    public SessionDialog(StationService stationService, UserService userService)
    {
        _stations = stationService.GetAll().Where(s => s.Status == StationStatus.Available).ToList();
        _users = userService.GetAll();
        InitializeLayout();
    }

    private void InitializeLayout()
    {
        Text = "Start New Session";
        Size = new Size(420, 280);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 2,
            RowCount = 6
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        void AddRow(int row, string label, Control ctrl)
        {
            table.Controls.Add(new Label { Text = label, TextAlign = ContentAlignment.MiddleLeft, AutoSize = false, Dock = DockStyle.Fill }, 0, row);
            ctrl.Dock = DockStyle.Fill;
            table.Controls.Add(ctrl, 1, row);
        }

        _cboStation.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboStation.DataSource = _stations;
        _cboStation.DisplayMember = "Name";
        _cboStation.ValueMember = "Id";

        _cboUser.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboUser.DataSource = _users;
        _cboUser.DisplayMember = "FullName";
        _cboUser.ValueMember = "Id";

        _txtNotes.Multiline = true;
        _txtNotes.Height = 40;

        AddRow(0, "Station *", _cboStation);
        AddRow(1, "User *", _cboUser);
        AddRow(2, "Purpose", _txtPurpose);
        AddRow(3, "Notes", _txtNotes);

        _btnOk.Text = "Start Session";
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
        table.Controls.Add(btnPanel, 0, 5);
        table.SetColumnSpan(btnPanel, 2);

        if (_stations.Count == 0)
        {
            var warn = new Label
            {
                Text = "⚠ No available stations at this time.",
                ForeColor = Color.Orange,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            table.Controls.Add(warn, 0, 4);
            table.SetColumnSpan(warn, 2);
            _btnOk.Enabled = false;
        }

        Controls.Add(table);
        AcceptButton = _btnOk;
        CancelButton = _btnCancel;
    }

    private void BtnOk_Click(object? sender, EventArgs e)
    {
        if (_cboStation.SelectedItem == null || _cboUser.SelectedItem == null)
        {
            MessageBox.Show("Please select a station and a user.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
            return;
        }

        Session.StationId = ((Station)_cboStation.SelectedItem).Id;
        Session.UserId = ((LabUser)_cboUser.SelectedItem).Id;
        Session.StartTime = DateTime.Now;
        Session.Purpose = _txtPurpose.Text.Trim();
        Session.Notes = _txtNotes.Text.Trim();
    }
}
