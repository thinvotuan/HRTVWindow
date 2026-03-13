namespace HRTVWindow.Forms;

partial class LoginForm
{
    private System.ComponentModel.IContainer components = null;

    private Label lblTitle;
    private Label lblUsername;
    private Label lblPassword;
    private TextBox txtUsername;
    private TextBox txtPassword;
    private Button btnLogin;
    private Panel pnlMain;
    private Label lblVersion;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        lblTitle = new Label();
        lblUsername = new Label();
        lblPassword = new Label();
        txtUsername = new TextBox();
        txtPassword = new TextBox();
        btnLogin = new Button();
        pnlMain = new Panel();
        lblVersion = new Label();

        // Form
        this.Text = "Đăng nhập - HRTVWindow";
        this.Size = new Size(420, 380);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.BackColor = Color.FromArgb(240, 244, 248);
        this.Font = new Font("Segoe UI", 9.5f);

        // pnlMain
        pnlMain.BackColor = Color.White;
        pnlMain.BorderStyle = BorderStyle.None;
        pnlMain.Location = new Point(30, 30);
        pnlMain.Size = new Size(360, 290);
        pnlMain.Padding = new Padding(20);

        // lblTitle
        lblTitle.Text = "🏢 HRTVWindow";
        lblTitle.Font = new Font("Segoe UI", 18f, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(0, 102, 204);
        lblTitle.AutoSize = false;
        lblTitle.Size = new Size(340, 45);
        lblTitle.Location = new Point(10, 15);
        lblTitle.TextAlign = ContentAlignment.MiddleCenter;

        // lblUsername
        lblUsername.Text = "Tên đăng nhập:";
        lblUsername.Location = new Point(30, 75);
        lblUsername.AutoSize = true;
        lblUsername.ForeColor = Color.FromArgb(60, 60, 60);

        // txtUsername
        txtUsername.Location = new Point(30, 95);
        txtUsername.Size = new Size(300, 30);
        txtUsername.Font = new Font("Segoe UI", 10.5f);
        txtUsername.BorderStyle = BorderStyle.FixedSingle;
        txtUsername.Text = "admin";

        // lblPassword
        lblPassword.Text = "Mật khẩu:";
        lblPassword.Location = new Point(30, 135);
        lblPassword.AutoSize = true;
        lblPassword.ForeColor = Color.FromArgb(60, 60, 60);

        // txtPassword
        txtPassword.Location = new Point(30, 155);
        txtPassword.Size = new Size(300, 30);
        txtPassword.Font = new Font("Segoe UI", 10.5f);
        txtPassword.PasswordChar = '●';
        txtPassword.BorderStyle = BorderStyle.FixedSingle;
        txtPassword.KeyDown += txtPassword_KeyDown;

        // btnLogin
        btnLogin.Text = "Đăng nhập";
        btnLogin.Location = new Point(30, 205);
        btnLogin.Size = new Size(300, 40);
        btnLogin.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
        btnLogin.BackColor = Color.FromArgb(0, 102, 204);
        btnLogin.ForeColor = Color.White;
        btnLogin.FlatStyle = FlatStyle.Flat;
        btnLogin.FlatAppearance.BorderSize = 0;
        btnLogin.Cursor = Cursors.Hand;
        btnLogin.Click += btnLogin_Click;

        pnlMain.Controls.AddRange(new Control[] { lblTitle, lblUsername, txtUsername, lblPassword, txtPassword, btnLogin });

        // lblVersion
        lblVersion.Text = "HRTVWindow v1.0 © 2024";
        lblVersion.AutoSize = true;
        lblVersion.ForeColor = Color.Gray;
        lblVersion.Font = new Font("Segoe UI", 8f);
        lblVersion.Location = new Point(130, 335);

        this.Controls.AddRange(new Control[] { pnlMain, lblVersion });

        ResumeLayout(false);
        PerformLayout();
    }
}
