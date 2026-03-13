using HRTVWindow.Data;
using HRTVWindow.Models;

namespace HRTVWindow.Forms;

public partial class LoginForm : Form
{
    private readonly UserRepository _userRepo = new();
    public User? LoggedInUser { get; private set; }

    public LoginForm()
    {
        InitializeComponent();
    }

    private void btnLogin_Click(object sender, EventArgs e)
    {
        string username = txtUsername.Text.Trim();
        string password = txtPassword.Text;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Vui lòng nhập tên đăng nhập và mật khẩu.", "Thông báo",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var user = _userRepo.Login(username, password);
        if (user != null)
        {
            LoggedInUser = user;
            DialogResult = DialogResult.OK;
            Close();
        }
        else
        {
            MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng.", "Lỗi đăng nhập",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            txtPassword.Clear();
            txtPassword.Focus();
        }
    }

    private void txtPassword_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
            btnLogin_Click(sender, e);
    }
}
