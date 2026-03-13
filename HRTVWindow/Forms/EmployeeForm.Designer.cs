namespace HRTVWindow.Forms;

partial class EmployeeForm
{
    private System.ComponentModel.IContainer components = null;

    private Label lblTitle;
    private Panel pnlTop;
    private Panel pnlLeft;
    private Panel pnlRight;
    private DataGridView dgvEmployees;
    private Label lblCount;
    private TextBox txtSearch;
    private Button btnSearch;
    private ComboBox cboFilterDept;
    private ComboBox cboFilterStatus;
    private Label lblSearch;
    private Label lblFilterDept;
    private Label lblFilterStatus;

    // Form fields
    private Label lblCode;
    private TextBox txtCode;
    private Label lblFullName;
    private TextBox txtFullName;
    private Label lblDOB;
    private DateTimePicker dtpDOB;
    private Label lblGender;
    private ComboBox cboGender;
    private Label lblIDCard;
    private TextBox txtIDCard;
    private Label lblPhone;
    private TextBox txtPhone;
    private Label lblEmail;
    private TextBox txtEmail;
    private Label lblAddress;
    private TextBox txtAddress;
    private Label lblDepartment;
    private ComboBox cboDepartment;
    private Label lblPosition;
    private ComboBox cboPosition;
    private Label lblJoinDate;
    private DateTimePicker dtpJoinDate;
    private Label lblStatus;
    private ComboBox cboStatus;
    private Button btnNew;
    private Button btnSave;
    private Button btnDelete;
    private Button btnClear;

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
        pnlTop = new Panel();
        pnlLeft = new Panel();
        pnlRight = new Panel();
        dgvEmployees = new DataGridView();
        lblCount = new Label();
        txtSearch = new TextBox();
        btnSearch = new Button();
        cboFilterDept = new ComboBox();
        cboFilterStatus = new ComboBox();
        lblSearch = new Label();
        lblFilterDept = new Label();
        lblFilterStatus = new Label();
        lblCode = new Label(); txtCode = new TextBox();
        lblFullName = new Label(); txtFullName = new TextBox();
        lblDOB = new Label(); dtpDOB = new DateTimePicker();
        lblGender = new Label(); cboGender = new ComboBox();
        lblIDCard = new Label(); txtIDCard = new TextBox();
        lblPhone = new Label(); txtPhone = new TextBox();
        lblEmail = new Label(); txtEmail = new TextBox();
        lblAddress = new Label(); txtAddress = new TextBox();
        lblDepartment = new Label(); cboDepartment = new ComboBox();
        lblPosition = new Label(); cboPosition = new ComboBox();
        lblJoinDate = new Label(); dtpJoinDate = new DateTimePicker();
        lblStatus = new Label(); cboStatus = new ComboBox();
        btnNew = new Button(); btnSave = new Button();
        btnDelete = new Button(); btnClear = new Button();

        SuspendLayout();

        // Form
        this.Text = "Quản lý Nhân viên";
        this.Size = new Size(1280, 720);
        this.MinimumSize = new Size(1100, 600);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(240, 244, 248);
        this.Font = new Font("Segoe UI", 9.5f);
        this.Load += EmployeeForm_Load;

        // lblTitle
        lblTitle.Text = "👥  QUẢN LÝ NHÂN VIÊN";
        lblTitle.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(0, 102, 204);
        lblTitle.AutoSize = true;
        lblTitle.Location = new Point(20, 15);

        // pnlTop - search bar
        pnlTop.BackColor = Color.White;
        pnlTop.Location = new Point(20, 50);
        pnlTop.Size = new Size(1230, 55);

        SetLbl(lblSearch, "Tìm kiếm:", new Point(10, 18));
        txtSearch.Location = new Point(80, 14);
        txtSearch.Size = new Size(200, 28);
        txtSearch.Font = new Font("Segoe UI", 10f);
        txtSearch.BorderStyle = BorderStyle.FixedSingle;
        txtSearch.KeyDown += txtSearch_KeyDown;

        SetBtn2(btnSearch, "🔍  Tìm", new Point(290, 12), Color.FromArgb(0, 102, 204), 90);
        btnSearch.Click += btnSearch_Click;

        SetLbl(lblFilterDept, "Phòng ban:", new Point(400, 18));
        cboFilterDept.Location = new Point(470, 14);
        cboFilterDept.Size = new Size(200, 28);
        cboFilterDept.Font = new Font("Segoe UI", 10f);
        cboFilterDept.DropDownStyle = ComboBoxStyle.DropDownList;
        cboFilterDept.SelectedIndexChanged += cboFilterDept_SelectedIndexChanged;

        SetLbl(lblFilterStatus, "Trạng thái:", new Point(685, 18));
        cboFilterStatus.Location = new Point(760, 14);
        cboFilterStatus.Size = new Size(160, 28);
        cboFilterStatus.Font = new Font("Segoe UI", 10f);
        cboFilterStatus.DropDownStyle = ComboBoxStyle.DropDownList;
        cboFilterStatus.Items.AddRange(new object[] { "Tất cả", "Đang làm việc", "Đã nghỉ việc" });
        cboFilterStatus.SelectedIndex = 0;
        cboFilterStatus.SelectedIndexChanged += cboFilterStatus_SelectedIndexChanged;

        pnlTop.Controls.AddRange(new Control[] { lblSearch, txtSearch, btnSearch, lblFilterDept, cboFilterDept, lblFilterStatus, cboFilterStatus });

        // pnlLeft - data grid
        pnlLeft.BackColor = Color.White;
        pnlLeft.Location = new Point(20, 115);
        pnlLeft.Size = new Size(730, 540);
        pnlLeft.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;

        dgvEmployees.Location = new Point(0, 0);
        dgvEmployees.Size = new Size(730, 510);
        dgvEmployees.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        dgvEmployees.ReadOnly = true;
        dgvEmployees.AllowUserToAddRows = false;
        dgvEmployees.AllowUserToDeleteRows = false;
        dgvEmployees.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvEmployees.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvEmployees.BackgroundColor = Color.White;
        dgvEmployees.BorderStyle = BorderStyle.None;
        dgvEmployees.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 102, 204);
        dgvEmployees.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvEmployees.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        dgvEmployees.EnableHeadersVisualStyles = false;
        dgvEmployees.RowTemplate.Height = 30;
        dgvEmployees.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 249, 255);
        dgvEmployees.SelectionChanged += dgvEmployees_SelectionChanged;

        lblCount.Text = "Tổng: 0 nhân viên";
        lblCount.Font = new Font("Segoe UI", 9f, FontStyle.Italic);
        lblCount.ForeColor = Color.Gray;
        lblCount.AutoSize = true;
        lblCount.Location = new Point(5, 515);

        pnlLeft.Controls.AddRange(new Control[] { dgvEmployees, lblCount });

        // pnlRight - form
        pnlRight.BackColor = Color.White;
        pnlRight.Location = new Point(760, 115);
        pnlRight.Size = new Size(490, 540);
        pnlRight.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
        pnlRight.AutoScroll = true;

        int y = 10;
        int lw = 460;

        AddFieldRow(pnlRight, lblCode, "Mã nhân viên: *", txtCode, ref y, lw);
        AddFieldRow(pnlRight, lblFullName, "Họ và tên: *", txtFullName, ref y, lw);

        // DOB and Gender in same row
        SetLbl2(lblDOB, "Ngày sinh:", new Point(15, y + 3));
        dtpDOB.Location = new Point(90, y);
        dtpDOB.Size = new Size(155, 28);
        dtpDOB.Format = DateTimePickerFormat.Short;
        dtpDOB.Value = DateTime.Today.AddYears(-25);

        SetLbl2(lblGender, "Giới tính:", new Point(255, y + 3));
        cboGender.Location = new Point(320, y);
        cboGender.Size = new Size(140, 28);
        cboGender.DropDownStyle = ComboBoxStyle.DropDownList;
        cboGender.Items.AddRange(new object[] { "Nam", "Nữ", "Khác" });
        cboGender.SelectedIndex = 0;
        y += 40;

        AddFieldRow(pnlRight, lblIDCard, "CMND/CCCD:", txtIDCard, ref y, lw);
        AddFieldRow(pnlRight, lblPhone, "Số điện thoại:", txtPhone, ref y, lw);
        AddFieldRow(pnlRight, lblEmail, "Email:", txtEmail, ref y, lw);
        AddFieldRow(pnlRight, lblAddress, "Địa chỉ:", txtAddress, ref y, lw);

        // Department
        SetLbl2(lblDepartment, "Phòng ban: *", new Point(15, y + 3));
        cboDepartment.Location = new Point(130, y);
        cboDepartment.Size = new Size(330, 28);
        cboDepartment.DropDownStyle = ComboBoxStyle.DropDownList;
        cboDepartment.SelectedIndexChanged += cboDepartment_SelectedIndexChanged;
        y += 40;

        // Position
        SetLbl2(lblPosition, "Chức vụ:", new Point(15, y + 3));
        cboPosition.Location = new Point(130, y);
        cboPosition.Size = new Size(330, 28);
        cboPosition.DropDownStyle = ComboBoxStyle.DropDownList;
        y += 40;

        // JoinDate and Status
        SetLbl2(lblJoinDate, "Ngày vào làm:", new Point(15, y + 3));
        dtpJoinDate.Location = new Point(130, y);
        dtpJoinDate.Size = new Size(155, 28);
        dtpJoinDate.Format = DateTimePickerFormat.Short;

        SetLbl2(lblStatus, "Trạng thái:", new Point(295, y + 3));
        cboStatus.Location = new Point(370, y);
        cboStatus.Size = new Size(90, 28);
        cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
        cboStatus.Items.AddRange(new object[] { "Đang làm", "Đã nghỉ" });
        cboStatus.SelectedIndex = 0;
        y += 50;

        // Buttons
        SetBtn2(btnNew, "🆕  Mới", new Point(15, y), Color.FromArgb(100, 100, 100), 100);
        btnNew.Click += btnNew_Click;
        SetBtn2(btnSave, "💾  Lưu", new Point(125, y), Color.FromArgb(0, 128, 0), 100);
        btnSave.Click += btnSave_Click;
        SetBtn2(btnDelete, "🗑️  Xóa", new Point(235, y), Color.FromArgb(180, 0, 0), 100);
        btnDelete.Click += btnDelete_Click;
        SetBtn2(btnClear, "🔄  Xóa form", new Point(345, y), Color.FromArgb(150, 80, 0), 120);
        btnClear.Click += btnClear_Click;

        pnlRight.Controls.AddRange(new Control[] {
            lblCode, txtCode, lblFullName, txtFullName,
            lblDOB, dtpDOB, lblGender, cboGender,
            lblIDCard, txtIDCard, lblPhone, txtPhone,
            lblEmail, txtEmail, lblAddress, txtAddress,
            lblDepartment, cboDepartment, lblPosition, cboPosition,
            lblJoinDate, dtpJoinDate, lblStatus, cboStatus,
            btnNew, btnSave, btnDelete, btnClear
        });

        // Filter dept combobox - load after departments are setup
        var deptRepo = new Data.DepartmentRepository();
        var filterDepts = new List<Models.Department> { new Models.Department { Id = 0, Name = "Tất cả phòng ban" } };
        filterDepts.AddRange(deptRepo.GetAll());
        cboFilterDept.DataSource = filterDepts;
        cboFilterDept.DisplayMember = "Name";
        cboFilterDept.ValueMember = "Id";
        cboFilterDept.SelectedIndex = 0;

        this.Controls.AddRange(new Control[] { lblTitle, pnlTop, pnlLeft, pnlRight });

        ResumeLayout(false);
        PerformLayout();
    }

    private static void AddFieldRow(Panel parent, Label lbl, string labelText, TextBox txt, ref int y, int width)
    {
        SetLbl2(lbl, labelText, new Point(15, y + 3));
        txt.Location = new Point(130, y);
        txt.Size = new Size(width - 130, 28);
        txt.Font = new Font("Segoe UI", 10f);
        txt.BorderStyle = BorderStyle.FixedSingle;
        y += 40;
    }

    private static void SetLbl2(Label lbl, string text, Point loc)
    {
        lbl.Text = text;
        lbl.AutoSize = true;
        lbl.Location = loc;
        lbl.ForeColor = Color.FromArgb(60, 60, 60);
    }

    private static void SetLbl(Label lbl, string text, Point loc)
    {
        lbl.Text = text;
        lbl.AutoSize = true;
        lbl.Location = loc;
        lbl.ForeColor = Color.FromArgb(60, 60, 60);
    }

    private static void SetBtn2(Button btn, string text, Point loc, Color color, int width = 120)
    {
        btn.Text = text;
        btn.Location = loc;
        btn.Size = new Size(width, 38);
        btn.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
        btn.BackColor = color;
        btn.ForeColor = Color.White;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.Cursor = Cursors.Hand;
    }
}
