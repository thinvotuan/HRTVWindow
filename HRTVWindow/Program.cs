using HRTVWindow.Data;
using HRTVWindow.Forms;

namespace HRTVWindow;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Initialize database
        string dbFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "HRTVWindow");
        Directory.CreateDirectory(dbFolder);
        string dbPath = Path.Combine(dbFolder, "hrtvwindow.db");
        DatabaseHelper.Initialize(dbPath);

        // Show login form
        using var loginForm = new LoginForm();
        if (loginForm.ShowDialog() == DialogResult.OK && loginForm.LoggedInUser != null)
        {
            Application.Run(new MainForm(loginForm.LoggedInUser));
        }
    }
}