using LabControl.Models;

namespace LabControl.Forms;

/// <summary>Dialog for adding or editing a Station.</summary>
public class StationDialog : Form
{
    private readonly TextBox _txtName = new();
    private readonly TextBox _txtIp = new();
    private readonly TextBox _txtLocation = new();
    private readonly ComboBox _cboStatus = new();
    private readonly TextBox _txtOs = new();
    private readonly TextBox _txtNotes = new();
    private readonly Button _btnOk = new();
    private readonly Button _btnCancel = new();

    public Station Station { get; private set; }

    public StationDialog(Station? station = null)
    {
        Station = station ?? new Station();
        InitializeLayout();
        if (station != null) PopulateFields();
    }

    private void InitializeLayout()
    {
        Text = Station.Id == 0 ? "Add Station" : "Edit Station";
        Size = new Size(400, 340);
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
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        void AddRow(int row, string label, Control ctrl)
        {
            table.Controls.Add(new Label { Text = label, TextAlign = ContentAlignment.MiddleLeft, AutoSize = false, Dock = DockStyle.Fill }, 0, row);
            ctrl.Dock = DockStyle.Fill;
            table.Controls.Add(ctrl, 1, row);
        }

        _cboStatus.Items.AddRange(Enum.GetNames(typeof(StationStatus)));
        _cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboStatus.SelectedIndex = 0;

        _txtNotes.Multiline = true;
        _txtNotes.Height = 50;

        AddRow(0, "Name *", _txtName);
        AddRow(1, "IP Address", _txtIp);
        AddRow(2, "Location", _txtLocation);
        AddRow(3, "Status", _cboStatus);
        AddRow(4, "Operating System", _txtOs);
        AddRow(5, "Notes", _txtNotes);

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
        _txtName.Text = Station.Name;
        _txtIp.Text = Station.IpAddress;
        _txtLocation.Text = Station.Location;
        _cboStatus.SelectedIndex = (int)Station.Status;
        _txtOs.Text = Station.OperatingSystem;
        _txtNotes.Text = Station.Notes;
    }

    private void BtnOk_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtName.Text))
        {
            MessageBox.Show("Station name is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
            return;
        }

        Station.Name = _txtName.Text.Trim();
        Station.IpAddress = _txtIp.Text.Trim();
        Station.Location = _txtLocation.Text.Trim();
        Station.Status = (StationStatus)_cboStatus.SelectedIndex;
        Station.OperatingSystem = _txtOs.Text.Trim();
        Station.Notes = _txtNotes.Text.Trim();
    }
}
