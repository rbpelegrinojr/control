using LabControl.Data;
using LabControl.Forms;
using LabControl.Models;
using LabControl.Services;

namespace LabControl;

public class MainForm : Form
{
    // Services
    private readonly DatabaseContext _db;
    private readonly StationService _stationService;
    private readonly UserService _userService;
    private readonly SessionService _sessionService;

    // Controls
    private TabControl _tabs = null!;

    // Dashboard
    private Label _lblTotalStations = null!;
    private Label _lblAvailableStations = null!;
    private Label _lblActiveSessionsCount = null!;
    private Label _lblTodaySessions = null!;
    private Label _lblTotalUsers = null!;

    // Stations tab
    private DataGridView _gridStations = null!;

    // Users tab
    private DataGridView _gridUsers = null!;
    private CheckBox _chkShowInactive = null!;

    // Sessions tab
    private DataGridView _gridSessions = null!;
    private CheckBox _chkActiveOnly = null!;
    private DateTimePicker _dtFrom = null!;
    private DateTimePicker _dtTo = null!;

    public MainForm(DatabaseContext db, StationService stations, UserService users, SessionService sessions)
    {
        _db = db;
        _stationService = stations;
        _userService = users;
        _sessionService = sessions;

        InitializeLayout();
        RefreshAll();
    }

    private void InitializeLayout()
    {
        Text = "Lab Control — Windows Lab Management System";
        Size = new Size(900, 600);
        MinimumSize = new Size(700, 500);
        StartPosition = FormStartPosition.CenterScreen;

        _tabs = new TabControl { Dock = DockStyle.Fill };

        _tabs.TabPages.Add(BuildDashboardTab());
        _tabs.TabPages.Add(BuildStationsTab());
        _tabs.TabPages.Add(BuildUsersTab());
        _tabs.TabPages.Add(BuildSessionsTab());

        Controls.Add(_tabs);

        var statusBar = new StatusStrip();
        var statusLabel = new ToolStripStatusLabel($"Lab Control v1.0  |  DB: {Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LabControl", "labcontrol.db")}");
        statusBar.Items.Add(statusLabel);
        Controls.Add(statusBar);
    }

    // ─── Dashboard Tab ───────────────────────────────────────────────────────────

    private TabPage BuildDashboardTab()
    {
        var page = new TabPage("Dashboard");
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true
        };

        _lblTotalStations = CreateStatCard("Total Stations", "0", Color.FromArgb(52, 152, 219));
        _lblAvailableStations = CreateStatCard("Available", "0", Color.FromArgb(39, 174, 96));
        _lblActiveSessionsCount = CreateStatCard("Active Sessions", "0", Color.FromArgb(230, 126, 34));
        _lblTodaySessions = CreateStatCard("Today's Sessions", "0", Color.FromArgb(142, 68, 173));
        _lblTotalUsers = CreateStatCard("Registered Users", "0", Color.FromArgb(41, 128, 185));

        panel.Controls.AddRange(new Control[]
        {
            _lblTotalStations, _lblAvailableStations, _lblActiveSessionsCount,
            _lblTodaySessions, _lblTotalUsers
        });
        page.Controls.Add(panel);
        return page;
    }

    private static Label CreateStatCard(string title, string value, Color color)
    {
        var lbl = new Label
        {
            Size = new Size(160, 100),
            Margin = new Padding(8),
            BackColor = color,
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            Text = $"{value}\n{title}"
        };
        return lbl;
    }

    // ─── Stations Tab ─────────────────────────────────────────────────────────────

    private TabPage BuildStationsTab()
    {
        var page = new TabPage("Stations");

        _gridStations = CreateGrid();
        _gridStations.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 45, ReadOnly = true },
            new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Name", Width = 150 },
            new DataGridViewTextBoxColumn { Name = "IpAddress", HeaderText = "IP Address", Width = 130 },
            new DataGridViewTextBoxColumn { Name = "Location", HeaderText = "Location", Width = 130 },
            new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 90 },
            new DataGridViewTextBoxColumn { Name = "OperatingSystem", HeaderText = "OS", Width = 120 },
            new DataGridViewTextBoxColumn { Name = "Notes", HeaderText = "Notes", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill }
        );

        var toolbar = CreateToolbar(
            ("Add", (s, e) => AddStation()),
            ("Edit", (s, e) => EditStation()),
            ("Delete", (s, e) => DeleteStation()),
            ("Refresh", (s, e) => LoadStations())
        );

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(_gridStations, 0, 1);
        page.Controls.Add(layout);
        return page;
    }

    // ─── Users Tab ───────────────────────────────────────────────────────────────

    private TabPage BuildUsersTab()
    {
        var page = new TabPage("Users");

        _gridUsers = CreateGrid();
        _gridUsers.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 45 },
            new DataGridViewTextBoxColumn { Name = "StudentId", HeaderText = "Student ID", Width = 120 },
            new DataGridViewTextBoxColumn { Name = "LastName", HeaderText = "Last Name", Width = 130 },
            new DataGridViewTextBoxColumn { Name = "FirstName", HeaderText = "First Name", Width = 130 },
            new DataGridViewTextBoxColumn { Name = "Department", HeaderText = "Department", Width = 130 },
            new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "Email", Width = 160 },
            new DataGridViewTextBoxColumn { Name = "IsActive", HeaderText = "Active", Width = 60 },
            new DataGridViewTextBoxColumn { Name = "DateRegistered", HeaderText = "Registered", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill }
        );

        _chkShowInactive = new CheckBox { Text = "Show inactive users", Margin = new Padding(6, 6, 0, 0) };
        _chkShowInactive.CheckedChanged += (s, e) => LoadUsers();

        var toolbar = CreateToolbar(
            ("Add", (s, e) => AddUser()),
            ("Edit", (s, e) => EditUser()),
            ("Delete", (s, e) => DeleteUser()),
            ("Refresh", (s, e) => LoadUsers())
        );
        toolbar.Controls.Add(_chkShowInactive);

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(_gridUsers, 0, 1);
        page.Controls.Add(layout);
        return page;
    }

    // ─── Sessions Tab ─────────────────────────────────────────────────────────────

    private TabPage BuildSessionsTab()
    {
        var page = new TabPage("Sessions");

        _gridSessions = CreateGrid();
        _gridSessions.Columns.AddRange(
            new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 45 },
            new DataGridViewTextBoxColumn { Name = "StationName", HeaderText = "Station", Width = 130 },
            new DataGridViewTextBoxColumn { Name = "UserStudentId", HeaderText = "Student ID", Width = 100 },
            new DataGridViewTextBoxColumn { Name = "UserFullName", HeaderText = "User", Width = 150 },
            new DataGridViewTextBoxColumn { Name = "StartTime", HeaderText = "Start", Width = 140 },
            new DataGridViewTextBoxColumn { Name = "EndTime", HeaderText = "End", Width = 140 },
            new DataGridViewTextBoxColumn { Name = "Duration", HeaderText = "Duration", Width = 80 },
            new DataGridViewTextBoxColumn { Name = "Purpose", HeaderText = "Purpose", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill }
        );

        _chkActiveOnly = new CheckBox { Text = "Active only", Checked = false, Margin = new Padding(6, 6, 0, 0) };
        _chkActiveOnly.CheckedChanged += (s, e) => LoadSessions();

        _dtFrom = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today.AddDays(-7), Width = 100, Margin = new Padding(6, 4, 2, 0) };
        _dtTo = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = DateTime.Today, Width = 100, Margin = new Padding(2, 4, 6, 0) };
        _dtFrom.ValueChanged += (s, e) => LoadSessions();
        _dtTo.ValueChanged += (s, e) => LoadSessions();

        var toolbar = CreateToolbar(
            ("New Session", (s, e) => StartSession()),
            ("End Session", (s, e) => EndSession()),
            ("Delete", (s, e) => DeleteSession()),
            ("Refresh", (s, e) => LoadSessions())
        );
        toolbar.Controls.Add(new Label { Text = "From:", Margin = new Padding(8, 8, 0, 0), AutoSize = true });
        toolbar.Controls.Add(_dtFrom);
        toolbar.Controls.Add(new Label { Text = "To:", Margin = new Padding(0, 8, 0, 0), AutoSize = true });
        toolbar.Controls.Add(_dtTo);
        toolbar.Controls.Add(_chkActiveOnly);

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(_gridSessions, 0, 1);
        page.Controls.Add(layout);
        return page;
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────────

    private static DataGridView CreateGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoGenerateColumns = false,
            BackgroundColor = SystemColors.Window,
            RowHeadersVisible = false,
            BorderStyle = BorderStyle.None
        };
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 152, 219);
        return grid;
    }

    private static FlowLayoutPanel CreateToolbar(params (string text, EventHandler handler)[] buttons)
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(4, 2, 4, 2),
            BackColor = Color.FromArgb(240, 240, 240)
        };
        foreach (var (text, handler) in buttons)
        {
            var btn = new Button { Text = text, AutoSize = true, Margin = new Padding(2, 2, 2, 2) };
            btn.Click += handler;
            panel.Controls.Add(btn);
        }
        return panel;
    }

    // ─── Data Loading ─────────────────────────────────────────────────────────────

    private void RefreshAll()
    {
        LoadDashboard();
        LoadStations();
        LoadUsers();
        LoadSessions();
    }

    private void LoadDashboard()
    {
        var counts = _stationService.GetStatusCounts();
        int total = counts.Values.Sum();
        int available = counts.TryGetValue(StationStatus.Available, out var a) ? a : 0;
        int activeSessions = _sessionService.GetActiveCount();
        int todaySessions = _sessionService.GetTodayCount();
        int totalUsers = _userService.GetTotalCount();

        UpdateStatCard(_lblTotalStations, total.ToString(), "Total Stations");
        UpdateStatCard(_lblAvailableStations, available.ToString(), "Available");
        UpdateStatCard(_lblActiveSessionsCount, activeSessions.ToString(), "Active Sessions");
        UpdateStatCard(_lblTodaySessions, todaySessions.ToString(), "Today's Sessions");
        UpdateStatCard(_lblTotalUsers, totalUsers.ToString(), "Registered Users");
    }

    private static void UpdateStatCard(Label lbl, string value, string title) =>
        lbl.Text = $"{value}\n{title}";

    private void LoadStations()
    {
        _gridStations.Rows.Clear();
        foreach (var s in _stationService.GetAll())
        {
            _gridStations.Rows.Add(s.Id, s.Name, s.IpAddress, s.Location,
                s.Status.ToString(), s.OperatingSystem, s.Notes);
        }
    }

    private void LoadUsers()
    {
        _gridUsers.Rows.Clear();
        foreach (var u in _userService.GetAll(_chkShowInactive.Checked))
        {
            _gridUsers.Rows.Add(u.Id, u.StudentId, u.LastName, u.FirstName,
                u.Department, u.Email, u.IsActive ? "Yes" : "No",
                u.DateRegistered.ToString("yyyy-MM-dd"));
        }
    }

    private void LoadSessions()
    {
        _gridSessions.Rows.Clear();
        IEnumerable<Session> sessions;
        if (_chkActiveOnly.Checked)
            sessions = _sessionService.GetActive();
        else
            sessions = _sessionService.GetAll(_dtFrom.Value.Date, _dtTo.Value.Date);

        foreach (var s in sessions)
        {
            _gridSessions.Rows.Add(
                s.Id, s.StationName, s.UserStudentId, s.UserFullName,
                s.StartTime.ToString("yyyy-MM-dd HH:mm"),
                s.EndTime?.ToString("yyyy-MM-dd HH:mm") ?? "—",
                s.Duration, s.Purpose);
        }
    }

    // ─── Stations CRUD ───────────────────────────────────────────────────────────

    private void AddStation()
    {
        using var dlg = new StationDialog();
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            _stationService.Add(dlg.Station);
            LoadStations();
            LoadDashboard();
        }
    }

    private void EditStation()
    {
        int? id = GetSelectedId(_gridStations);
        if (id == null) return;
        var station = _stationService.GetById(id.Value);
        if (station == null) return;

        using var dlg = new StationDialog(station);
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            _stationService.Update(dlg.Station);
            LoadStations();
            LoadDashboard();
        }
    }

    private void DeleteStation()
    {
        int? id = GetSelectedId(_gridStations);
        if (id == null) return;
        if (Confirm("Delete this station?"))
        {
            _stationService.Delete(id.Value);
            LoadStations();
            LoadDashboard();
        }
    }

    // ─── Users CRUD ──────────────────────────────────────────────────────────────

    private void AddUser()
    {
        using var dlg = new UserDialog();
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                _userService.Add(dlg.User);
                LoadUsers();
                LoadDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not add user: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void EditUser()
    {
        int? id = GetSelectedId(_gridUsers);
        if (id == null) return;
        var user = _userService.GetById(id.Value);
        if (user == null) return;

        using var dlg = new UserDialog(user);
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                _userService.Update(dlg.User);
                LoadUsers();
                LoadDashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not update user: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void DeleteUser()
    {
        int? id = GetSelectedId(_gridUsers);
        if (id == null) return;
        if (Confirm("Delete this user? Their session history will also be removed."))
        {
            _userService.Delete(id.Value);
            LoadUsers();
            LoadDashboard();
        }
    }

    // ─── Sessions CRUD ───────────────────────────────────────────────────────────

    private void StartSession()
    {
        using var dlg = new SessionDialog(_stationService, _userService);
        if (dlg.ShowDialog(this) == DialogResult.OK)
        {
            _sessionService.Start(dlg.Session);
            LoadSessions();
            LoadDashboard();
            LoadStations();
        }
    }

    private void EndSession()
    {
        int? id = GetSelectedId(_gridSessions);
        if (id == null) return;
        if (Confirm("End this session?"))
        {
            _sessionService.End(id.Value);
            LoadSessions();
            LoadDashboard();
            LoadStations();
        }
    }

    private void DeleteSession()
    {
        int? id = GetSelectedId(_gridSessions);
        if (id == null) return;
        if (Confirm("Delete this session record?"))
        {
            _sessionService.Delete(id.Value);
            LoadSessions();
            LoadDashboard();
        }
    }

    // ─── Utilities ────────────────────────────────────────────────────────────────

    private static int? GetSelectedId(DataGridView grid)
    {
        if (grid.SelectedRows.Count == 0)
        {
            MessageBox.Show("Please select a row first.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return null;
        }
        return Convert.ToInt32(grid.SelectedRows[0].Cells["Id"].Value);
    }

    private bool Confirm(string message) =>
        MessageBox.Show(message, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        _db.Dispose();
    }
}
