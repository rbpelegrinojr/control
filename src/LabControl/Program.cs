using LabControl.Data;
using LabControl.Forms;
using LabControl.Services;

namespace LabControl;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            var db = new DatabaseContext();
            var stations = new StationService(db);
            var users = new UserService(db);
            var sessions = new SessionService(db);

            Application.Run(new MainForm(db, stations, users, sessions));
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to initialize Lab Control:\n\n{ex.Message}",
                "Startup Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}