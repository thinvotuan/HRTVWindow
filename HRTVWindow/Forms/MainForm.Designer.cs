namespace HRTVWindow.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private StatusStrip statusStrip;
    private ToolStripStatusLabel lblWelcome;
    private ToolStripStatusLabel lblDateTime;
    private Panel pnlSidebar;
    private Panel pnlContent;
    private Panel pnlHeader;
    private Label lblAppTitle;
    private Button btnEmployees;
    private Button btnDepartments;
    private Button btnPositions;
    private Button btnContracts;
    private Button btnSalaries;
    private Button btnAttendance;
    private Button btnRefresh;
    private Button btnLogout;
    private DataGridView dgvDashboard;
    private Panel pnlStats;
    private Label lblStatEmp;
    private Label lblTotalEmployees;
    private Label lblStatDept;
    private Label lblTotalDepartments;
    private Label lblStatDate;
    private Label lblCurrentDate;
    private System.Windows.Forms.Timer timerClock;
    private Label lblDashboardTitle;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        pnlHeader = new Panel();
        lblAppTitle = new Label();
        pnlSidebar = new Panel();
        pnlContent = new Panel();
        pnlStats = new Panel();
        statusStrip = new StatusStrip();
        lblWelcome = new ToolStripStatusLabel();
        lblDateTime = new ToolStripStatusLabel();
        btnEmployees = new Button();
        btnDepartments = new Button();
        btnPositions = new Button();
        btnContracts = new Button();
        btnSalaries = new Button();
        btnAttendance = new Button();
        btnRefresh = new Button();
        btnLogout = new Button();
        dgvDashboard = new DataGridView();
        lblStatEmp = new Label();
        lblTotalEmployees = new Label();
        lblStatDept = new Label();
        lblTotalDepartments = new Label();
        lblStatDate = new Label();
        lblCurrentDate = new Label();
        lblDashboardTitle = new Label();
        timerClock = new System.Windows.Forms.Timer(components);

        SuspendLayout();

        // Form
        this.Text = "HRTVWindow - Hệ thống quản lý nhân sự";
        this.Size = new Size(1200, 720);
        this.MinimumSize = new Size(1000, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(240, 244, 248);
        this.Font = new Font("Segoe UI", 9.5f);
        this.Load += MainForm_Load;

        // timerClock
        timerClock.Interval = 1000;
        timerClock.Tick += timerClock_Tick;

        // statusStrip
        statusStrip.BackColor = Color.FromArgb(0, 102, 204);
        statusStrip.ForeColor = Color.White;
        statusStrip.Items.AddRange(new ToolStripItem[] { lblWelcome, new ToolStripStatusLabel { Spring = true }, lblDateTime });
        lblWelcome.ForeColor = Color.White;
        lblDateTime.ForeColor = Color.White;

        // pnlHeader
        pnlHeader.BackColor = Color.FromArgb(0, 102, 204);
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.Height = 60;
        pnlHeader.Padding = new Padding(10, 0, 10, 0);

        // lblAppTitle
        lblAppTitle.Text = "🏢  HRTVWindow - Hệ thống Quản lý Nhân sự";
        lblAppTitle.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
        lblAppTitle.ForeColor = Color.White;
        lblAppTitle.AutoSize = true;
        lblAppTitle.Location = new Point(15, 13);
        pnlHeader.Controls.Add(lblAppTitle);

        // pnlSidebar
        pnlSidebar.BackColor = Color.FromArgb(30, 42, 60);
        pnlSidebar.Dock = DockStyle.Left;
        pnlSidebar.Width = 220;
        pnlSidebar.Padding = new Padding(0, 10, 0, 10);

        var sidebarButtons = new[]
        {
            (btnEmployees, "👥  Nhân viên", nameof(btnEmployees_Click)),
            (btnDepartments, "🏬  Phòng ban", nameof(btnDepartments_Click)),
            (btnPositions, "💼  Chức vụ", nameof(btnPositions_Click)),
            (btnContracts, "📋  Hợp đồng", nameof(btnContracts_Click)),
            (btnSalaries, "💰  Bảng lương", nameof(btnSalaries_Click)),
            (btnAttendance, "📅  Chấm công", nameof(btnAttendance_Click)),
        };

        int yPos = 20;
        foreach (var (btn, text, _) in sidebarButtons)
        {
            ConfigureSidebarButton(btn, text, yPos);
            yPos += 52;
        }
        btnEmployees.Click += btnEmployees_Click;
        btnDepartments.Click += btnDepartments_Click;
        btnPositions.Click += btnPositions_Click;
        btnContracts.Click += btnContracts_Click;
        btnSalaries.Click += btnSalaries_Click;
        btnAttendance.Click += btnAttendance_Click;

        // Refresh button
        ConfigureSidebarButton(btnRefresh, "🔄  Làm mới", 0);
        btnRefresh.Location = new Point(10, yPos + 20);
        btnRefresh.Click += btnRefresh_Click;

        // Logout button
        ConfigureSidebarButton(btnLogout, "🚪  Đăng xuất", 0);
        btnLogout.BackColor = Color.FromArgb(180, 30, 30);
        btnLogout.Location = new Point(10, yPos + 80);
        btnLogout.Click += btnLogout_Click;

        pnlSidebar.Controls.AddRange(new Control[] { btnEmployees, btnDepartments, btnPositions, btnContracts, btnSalaries, btnAttendance, btnRefresh, btnLogout });

        // pnlContent
        pnlContent.BackColor = Color.FromArgb(240, 244, 248);
        pnlContent.Dock = DockStyle.Fill;
        pnlContent.Padding = new Padding(20);

        // Stats panel
        pnlStats.BackColor = Color.Transparent;
        pnlStats.Location = new Point(20, 20);
        pnlStats.Size = new Size(900, 110);

        CreateStatCard(pnlStats, 0, "👥 Tổng nhân viên", lblStatEmp, lblTotalEmployees, Color.FromArgb(0, 120, 215));
        CreateStatCard(pnlStats, 1, "🏬 Phòng ban", lblStatDept, lblTotalDepartments, Color.FromArgb(16, 124, 16));
        CreateStatCard(pnlStats, 2, "📅 Ngày hiện tại", lblStatDate, lblCurrentDate, Color.FromArgb(200, 112, 0));

        // Dashboard title
        lblDashboardTitle.Text = "Nhân viên gần đây";
        lblDashboardTitle.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
        lblDashboardTitle.ForeColor = Color.FromArgb(30, 42, 60);
        lblDashboardTitle.AutoSize = true;
        lblDashboardTitle.Location = new Point(20, 145);

        // DataGridView
        dgvDashboard.Location = new Point(20, 175);
        dgvDashboard.Size = new Size(900, 350);
        dgvDashboard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        dgvDashboard.ReadOnly = true;
        dgvDashboard.AllowUserToAddRows = false;
        dgvDashboard.AllowUserToDeleteRows = false;
        dgvDashboard.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvDashboard.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvDashboard.BackgroundColor = Color.White;
        dgvDashboard.BorderStyle = BorderStyle.None;
        dgvDashboard.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 102, 204);
        dgvDashboard.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvDashboard.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        dgvDashboard.EnableHeadersVisualStyles = false;
        dgvDashboard.RowTemplate.Height = 30;
        dgvDashboard.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 249, 255);

        pnlContent.Controls.AddRange(new Control[] { pnlStats, lblDashboardTitle, dgvDashboard });

        this.Controls.AddRange(new Control[] { pnlContent, pnlSidebar, pnlHeader, statusStrip });

        ResumeLayout(false);
        PerformLayout();
    }

    private static void ConfigureSidebarButton(Button btn, string text, int yPos)
    {
        btn.Text = text;
        btn.Size = new Size(200, 42);
        btn.Location = new Point(10, yPos);
        btn.Font = new Font("Segoe UI", 10f);
        btn.BackColor = Color.FromArgb(45, 60, 80);
        btn.ForeColor = Color.White;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.TextAlign = ContentAlignment.MiddleLeft;
        btn.Padding = new Padding(10, 0, 0, 0);
        btn.Cursor = Cursors.Hand;
    }

    private static void CreateStatCard(Panel parent, int index, string title, Label lblTitle, Label lblValue, Color color)
    {
        int x = index * 210;
        var card = new Panel
        {
            Location = new Point(x, 0),
            Size = new Size(200, 100),
            BackColor = Color.White
        };

        var accent = new Panel
        {
            Location = new Point(0, 0),
            Size = new Size(6, 100),
            BackColor = color
        };

        lblTitle.Text = title;
        lblTitle.Font = new Font("Segoe UI", 9f);
        lblTitle.ForeColor = Color.Gray;
        lblTitle.AutoSize = true;
        lblTitle.Location = new Point(15, 15);

        lblValue.Text = "0";
        lblValue.Font = new Font("Segoe UI", 22f, FontStyle.Bold);
        lblValue.ForeColor = color;
        lblValue.AutoSize = true;
        lblValue.Location = new Point(15, 45);

        card.Controls.AddRange(new Control[] { accent, lblTitle, lblValue });
        parent.Controls.Add(card);
    }
}
