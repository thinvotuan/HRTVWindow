namespace HRTVWindow.Forms;

partial class ContractForm
{
    private System.ComponentModel.IContainer components = null;

    private Label lblTitle;
    private Panel pnlTop;
    private Panel pnlLeft;
    private Panel pnlRight;
    private DataGridView dgvContracts;
    private Label lblFilterEmployee;
    private ComboBox cboFilterEmployee;

    private Label lblEmployee;
    private ComboBox cboEmployee;
    private Label lblContractNumber;
    private TextBox txtContractNumber;
    private Label lblContractType;
    private ComboBox cboContractType;
    private Label lblStartDate;
    private DateTimePicker dtpStartDate;
    private Label lblEndDate;
    private DateTimePicker dtpEndDate;
    private CheckBox chkNoEndDate;
    private Label lblBasicSalary;
    private TextBox txtBasicSalary;
    private Label lblNotes;
    private TextBox txtNotes;
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
        dgvContracts = new DataGridView();
        lblFilterEmployee = new Label();
        cboFilterEmployee = new ComboBox();
        lblEmployee = new Label(); cboEmployee = new ComboBox();
        lblContractNumber = new Label(); txtContractNumber = new TextBox();
        lblContractType = new Label(); cboContractType = new ComboBox();
        lblStartDate = new Label(); dtpStartDate = new DateTimePicker();
        lblEndDate = new Label(); dtpEndDate = new DateTimePicker();
        chkNoEndDate = new CheckBox();
        lblBasicSalary = new Label(); txtBasicSalary = new TextBox();
        lblNotes = new Label(); txtNotes = new TextBox();
        btnSave = new Button(); btnDelete = new Button(); btnClear = new Button();

        SuspendLayout();

        this.Text = "Quản lý Hợp đồng";
        this.Size = new Size(1200, 680);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(240, 244, 248);
        this.Font = new Font("Segoe UI", 9.5f);
        this.Load += ContractForm_Load;

        // Title
        lblTitle.Text = "📋  QUẢN LÝ HỢP ĐỒNG LAO ĐỘNG";
        lblTitle.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(0, 102, 204);
        lblTitle.AutoSize = true;
        lblTitle.Location = new Point(20, 15);

        // pnlTop - filter
        pnlTop.BackColor = Color.White;
        pnlTop.Location = new Point(20, 50);
        pnlTop.Size = new Size(1150, 50);

        lblFilterEmployee.Text = "Lọc theo nhân viên:";
        lblFilterEmployee.AutoSize = true;
        lblFilterEmployee.Location = new Point(10, 15);

        var empList = new List<Models.Employee> { new Models.Employee { Id = 0, EmployeeCode = "", FullName = "Tất cả nhân viên" } };
        empList.AddRange(new Data.EmployeeRepository().GetAll(status: 1));
        cboFilterEmployee.DataSource = empList;
        cboFilterEmployee.DisplayMember = "ToString";
        cboFilterEmployee.ValueMember = "Id";
        cboFilterEmployee.Location = new Point(160, 12);
        cboFilterEmployee.Size = new Size(250, 28);
        cboFilterEmployee.DropDownStyle = ComboBoxStyle.DropDownList;
        cboFilterEmployee.SelectedIndexChanged += cboFilterEmployee_SelectedIndexChanged;
        pnlTop.Controls.AddRange(new Control[] { lblFilterEmployee, cboFilterEmployee });

        // pnlLeft - grid
        pnlLeft.BackColor = Color.White;
        pnlLeft.Location = new Point(20, 110);
        pnlLeft.Size = new Size(720, 510);
        pnlLeft.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;

        dgvContracts.Dock = DockStyle.Fill;
        dgvContracts.ReadOnly = true;
        dgvContracts.AllowUserToAddRows = false;
        dgvContracts.AllowUserToDeleteRows = false;
        dgvContracts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvContracts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvContracts.BackgroundColor = Color.White;
        dgvContracts.BorderStyle = BorderStyle.None;
        dgvContracts.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 102, 204);
        dgvContracts.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvContracts.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        dgvContracts.EnableHeadersVisualStyles = false;
        dgvContracts.RowTemplate.Height = 30;
        dgvContracts.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 249, 255);
        dgvContracts.SelectionChanged += dgvContracts_SelectionChanged;
        pnlLeft.Controls.Add(dgvContracts);

        // pnlRight - form
        pnlRight.BackColor = Color.White;
        pnlRight.Location = new Point(755, 110);
        pnlRight.Size = new Size(415, 510);
        pnlRight.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;

        int y = 15;
        AddRow(pnlRight, lblEmployee, "Nhân viên: *", ref y);
        cboEmployee.Location = new Point(130, y - 28);
        cboEmployee.Size = new Size(270, 28);
        cboEmployee.DropDownStyle = ComboBoxStyle.DropDownList;

        AddRow(pnlRight, lblContractNumber, "Số hợp đồng: *", ref y);
        txtContractNumber.Location = new Point(130, y - 28);
        txtContractNumber.Size = new Size(270, 28);
        txtContractNumber.Font = new Font("Segoe UI", 10f);
        txtContractNumber.BorderStyle = BorderStyle.FixedSingle;

        AddRow(pnlRight, lblContractType, "Loại hợp đồng:", ref y);
        cboContractType.Location = new Point(130, y - 28);
        cboContractType.Size = new Size(270, 28);
        cboContractType.DropDownStyle = ComboBoxStyle.DropDownList;
        cboContractType.Items.AddRange(new object[] { "Thử việc", "Ngắn hạn (1 năm)", "Dài hạn (3 năm)", "Không thời hạn" });
        cboContractType.SelectedIndex = 0;

        AddRow(pnlRight, lblStartDate, "Ngày bắt đầu:", ref y);
        dtpStartDate.Location = new Point(130, y - 28);
        dtpStartDate.Size = new Size(270, 28);
        dtpStartDate.Format = DateTimePickerFormat.Short;

        AddRow(pnlRight, lblEndDate, "Ngày kết thúc:", ref y);
        dtpEndDate.Location = new Point(130, y - 28);
        dtpEndDate.Size = new Size(200, 28);
        dtpEndDate.Format = DateTimePickerFormat.Short;
        dtpEndDate.Value = DateTime.Today.AddYears(1);

        chkNoEndDate.Text = "Không thời hạn";
        chkNoEndDate.Location = new Point(130, y);
        chkNoEndDate.AutoSize = true;
        chkNoEndDate.CheckedChanged += chkNoEndDate_CheckedChanged;
        y += 35;

        AddRow(pnlRight, lblBasicSalary, "Lương cơ bản:", ref y);
        txtBasicSalary.Location = new Point(130, y - 28);
        txtBasicSalary.Size = new Size(270, 28);
        txtBasicSalary.Font = new Font("Segoe UI", 10f);
        txtBasicSalary.BorderStyle = BorderStyle.FixedSingle;
        txtBasicSalary.Text = "0";

        AddRow(pnlRight, lblNotes, "Ghi chú:", ref y);
        txtNotes.Location = new Point(130, y - 28);
        txtNotes.Size = new Size(270, 60);
        txtNotes.Font = new Font("Segoe UI", 10f);
        txtNotes.Multiline = true;
        txtNotes.BorderStyle = BorderStyle.FixedSingle;
        y += 45;

        SetBtn3(btnSave, "💾  Lưu", new Point(15, y), Color.FromArgb(0, 128, 0));
        btnSave.Click += btnSave_Click;
        SetBtn3(btnDelete, "🗑️  Xóa", new Point(155, y), Color.FromArgb(180, 0, 0));
        btnDelete.Click += btnDelete_Click;
        SetBtn3(btnClear, "🔄  Mới", new Point(295, y), Color.FromArgb(100, 100, 100));
        btnClear.Click += btnClear_Click;

        pnlRight.Controls.AddRange(new Control[] {
            lblEmployee, cboEmployee, lblContractNumber, txtContractNumber,
            lblContractType, cboContractType, lblStartDate, dtpStartDate,
            lblEndDate, dtpEndDate, chkNoEndDate,
            lblBasicSalary, txtBasicSalary, lblNotes, txtNotes,
            btnSave, btnDelete, btnClear
        });

        this.Controls.AddRange(new Control[] { lblTitle, pnlTop, pnlLeft, pnlRight });

        ResumeLayout(false);
        PerformLayout();
    }

    private static void AddRow(Panel p, Label lbl, string text, ref int y)
    {
        lbl.Text = text;
        lbl.AutoSize = true;
        lbl.Location = new Point(15, y + 5);
        lbl.ForeColor = Color.FromArgb(60, 60, 60);
        y += 40;
    }

    private static void SetBtn3(Button btn, string text, Point loc, Color color)
    {
        btn.Text = text;
        btn.Location = loc;
        btn.Size = new Size(120, 38);
        btn.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
        btn.BackColor = color;
        btn.ForeColor = Color.White;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.Cursor = Cursors.Hand;
    }
}
