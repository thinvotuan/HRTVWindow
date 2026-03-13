namespace HRTVWindow.Forms;

partial class AttendanceForm
{
    private System.ComponentModel.IContainer components = null;

    private Label lblTitle;
    private Panel pnlTop;
    private Panel pnlLeft;
    private Panel pnlRight;
    private DataGridView dgvAttendance;
    private Label lblCount;

    // Filters
    private CheckBox chkFilterByDate;
    private DateTimePicker dtpFilterDate;
    private CheckBox chkFilterByMonth;
    private Label lblFilterMonth;
    private NumericUpDown numFilterMonth;
    private Label lblFilterYear;
    private NumericUpDown numFilterYear;
    private Label lblFilterEmployee;
    private ComboBox cboFilterEmployee;
    private Button btnSearch;

    // Form fields
    private Label lblEmployee;
    private ComboBox cboEmployee;
    private Label lblDate;
    private DateTimePicker dtpDate;
    private Label lblCheckIn;
    private TextBox txtCheckIn;
    private Label lblCheckOut;
    private TextBox txtCheckOut;
    private Label lblStatus;
    private ComboBox cboStatus;
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
        dgvAttendance = new DataGridView();
        lblCount = new Label();
        chkFilterByDate = new CheckBox(); dtpFilterDate = new DateTimePicker();
        chkFilterByMonth = new CheckBox();
        lblFilterMonth = new Label(); numFilterMonth = new NumericUpDown();
        lblFilterYear = new Label(); numFilterYear = new NumericUpDown();
        lblFilterEmployee = new Label(); cboFilterEmployee = new ComboBox();
        btnSearch = new Button();
        lblEmployee = new Label(); cboEmployee = new ComboBox();
        lblDate = new Label(); dtpDate = new DateTimePicker();
        lblCheckIn = new Label(); txtCheckIn = new TextBox();
        lblCheckOut = new Label(); txtCheckOut = new TextBox();
        lblStatus = new Label(); cboStatus = new ComboBox();
        lblNotes = new Label(); txtNotes = new TextBox();
        btnSave = new Button(); btnDelete = new Button(); btnClear = new Button();

        SuspendLayout();

        this.Text = "Quản lý Chấm công";
        this.Size = new Size(1280, 700);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(240, 244, 248);
        this.Font = new Font("Segoe UI", 9.5f);
        this.Load += AttendanceForm_Load;

        // Title
        lblTitle.Text = "📅  QUẢN LÝ CHẤM CÔNG";
        lblTitle.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(0, 102, 204);
        lblTitle.AutoSize = true;
        lblTitle.Location = new Point(20, 15);

        // pnlTop
        pnlTop.BackColor = Color.White;
        pnlTop.Location = new Point(20, 50);
        pnlTop.Size = new Size(1230, 55);

        chkFilterByDate.Text = "Theo ngày:";
        chkFilterByDate.AutoSize = true;
        chkFilterByDate.Location = new Point(10, 17);
        chkFilterByDate.Checked = true;
        chkFilterByDate.CheckedChanged += chkFilterByDate_CheckedChanged;

        dtpFilterDate.Location = new Point(100, 13);
        dtpFilterDate.Size = new Size(130, 28);
        dtpFilterDate.Format = DateTimePickerFormat.Short;

        chkFilterByMonth.Text = "Theo tháng:";
        chkFilterByMonth.AutoSize = true;
        chkFilterByMonth.Location = new Point(245, 17);
        chkFilterByMonth.CheckedChanged += chkFilterByMonth_CheckedChanged;

        lblFilterMonth.Text = "Tháng";
        lblFilterMonth.AutoSize = true;
        lblFilterMonth.Location = new Point(350, 17);
        numFilterMonth.Location = new Point(395, 13);
        numFilterMonth.Size = new Size(55, 28);
        numFilterMonth.Minimum = 1;
        numFilterMonth.Maximum = 12;
        numFilterMonth.Value = DateTime.Today.Month;
        numFilterMonth.Enabled = false;

        lblFilterYear.Text = "Năm";
        lblFilterYear.AutoSize = true;
        lblFilterYear.Location = new Point(460, 17);
        numFilterYear.Location = new Point(490, 13);
        numFilterYear.Size = new Size(75, 28);
        numFilterYear.Minimum = 2000;
        numFilterYear.Maximum = 2100;
        numFilterYear.Value = DateTime.Today.Year;
        numFilterYear.Enabled = false;

        lblFilterEmployee.Text = "Nhân viên:";
        lblFilterEmployee.AutoSize = true;
        lblFilterEmployee.Location = new Point(580, 17);
        var empList2 = new List<Models.Employee> { new Models.Employee { Id = 0, EmployeeCode = "", FullName = "Tất cả" } };
        empList2.AddRange(new Data.EmployeeRepository().GetAll());
        cboFilterEmployee.DataSource = empList2;
        cboFilterEmployee.DisplayMember = "ToString";
        cboFilterEmployee.ValueMember = "Id";
        cboFilterEmployee.Location = new Point(645, 13);
        cboFilterEmployee.Size = new Size(230, 28);
        cboFilterEmployee.DropDownStyle = ComboBoxStyle.DropDownList;

        SetBtn5(btnSearch, "🔍  Tìm", new Point(890, 12), Color.FromArgb(0, 102, 204), 90);
        btnSearch.Click += btnSearch_Click;

        pnlTop.Controls.AddRange(new Control[] { chkFilterByDate, dtpFilterDate, chkFilterByMonth, lblFilterMonth, numFilterMonth, lblFilterYear, numFilterYear, lblFilterEmployee, cboFilterEmployee, btnSearch });

        // pnlLeft
        pnlLeft.BackColor = Color.White;
        pnlLeft.Location = new Point(20, 115);
        pnlLeft.Size = new Size(800, 530);
        pnlLeft.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;

        dgvAttendance.Dock = DockStyle.Fill;
        dgvAttendance.ReadOnly = true;
        dgvAttendance.AllowUserToAddRows = false;
        dgvAttendance.AllowUserToDeleteRows = false;
        dgvAttendance.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvAttendance.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvAttendance.BackgroundColor = Color.White;
        dgvAttendance.BorderStyle = BorderStyle.None;
        dgvAttendance.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 102, 204);
        dgvAttendance.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvAttendance.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        dgvAttendance.EnableHeadersVisualStyles = false;
        dgvAttendance.RowTemplate.Height = 30;
        dgvAttendance.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 249, 255);
        dgvAttendance.SelectionChanged += dgvAttendance_SelectionChanged;

        lblCount.Text = "Tổng: 0 bản ghi";
        lblCount.Font = new Font("Segoe UI", 9f, FontStyle.Italic);
        lblCount.ForeColor = Color.Gray;
        lblCount.AutoSize = true;
        lblCount.Dock = DockStyle.Bottom;

        pnlLeft.Controls.AddRange(new Control[] { dgvAttendance, lblCount });

        // pnlRight
        pnlRight.BackColor = Color.White;
        pnlRight.Location = new Point(835, 115);
        pnlRight.Size = new Size(395, 530);
        pnlRight.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;

        int y = 15;
        AddAttRow(pnlRight, lblEmployee, "Nhân viên:", ref y);
        var emps2 = new Data.EmployeeRepository().GetAll(status: 1);
        cboEmployee.DataSource = emps2;
        cboEmployee.DisplayMember = "ToString";
        cboEmployee.ValueMember = "Id";
        cboEmployee.Location = new Point(120, y - 28);
        cboEmployee.Size = new Size(260, 28);
        cboEmployee.DropDownStyle = ComboBoxStyle.DropDownList;

        AddAttRow(pnlRight, lblDate, "Ngày:", ref y);
        dtpDate.Location = new Point(120, y - 28);
        dtpDate.Size = new Size(260, 28);
        dtpDate.Format = DateTimePickerFormat.Short;

        AddAttRow(pnlRight, lblCheckIn, "Giờ vào:", ref y);
        txtCheckIn.Location = new Point(120, y - 28);
        txtCheckIn.Size = new Size(120, 28);
        txtCheckIn.Font = new Font("Segoe UI", 10f);
        txtCheckIn.BorderStyle = BorderStyle.FixedSingle;
        txtCheckIn.Text = "08:00";

        AddAttRow(pnlRight, lblCheckOut, "Giờ ra:", ref y);
        txtCheckOut.Location = new Point(120, y - 28);
        txtCheckOut.Size = new Size(120, 28);
        txtCheckOut.Font = new Font("Segoe UI", 10f);
        txtCheckOut.BorderStyle = BorderStyle.FixedSingle;
        txtCheckOut.Text = "17:30";

        AddAttRow(pnlRight, lblStatus, "Trạng thái:", ref y);
        cboStatus.Location = new Point(120, y - 28);
        cboStatus.Size = new Size(260, 28);
        cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
        cboStatus.Items.AddRange(new object[] { "Có mặt", "Đi trễ", "Vắng mặt", "Nghỉ phép", "Nghỉ lễ" });
        cboStatus.SelectedIndex = 0;

        AddAttRow(pnlRight, lblNotes, "Ghi chú:", ref y);
        txtNotes.Location = new Point(120, y - 28);
        txtNotes.Size = new Size(260, 55);
        txtNotes.Font = new Font("Segoe UI", 10f);
        txtNotes.Multiline = true;
        txtNotes.BorderStyle = BorderStyle.FixedSingle;
        y += 30;

        SetBtn5(btnSave, "💾  Lưu", new Point(15, y), Color.FromArgb(0, 128, 0), 110);
        btnSave.Click += btnSave_Click;
        SetBtn5(btnDelete, "🗑️  Xóa", new Point(135, y), Color.FromArgb(180, 0, 0), 110);
        btnDelete.Click += btnDelete_Click;
        SetBtn5(btnClear, "🔄  Mới", new Point(255, y), Color.FromArgb(100, 100, 100), 110);
        btnClear.Click += btnClear_Click;

        pnlRight.Controls.AddRange(new Control[] {
            lblEmployee, cboEmployee, lblDate, dtpDate,
            lblCheckIn, txtCheckIn, lblCheckOut, txtCheckOut,
            lblStatus, cboStatus, lblNotes, txtNotes,
            btnSave, btnDelete, btnClear
        });

        this.Controls.AddRange(new Control[] { lblTitle, pnlTop, pnlLeft, pnlRight });

        ResumeLayout(false);
        PerformLayout();
    }

    private static void AddAttRow(Panel p, Label lbl, string text, ref int y)
    {
        lbl.Text = text;
        lbl.AutoSize = true;
        lbl.Location = new Point(15, y + 5);
        lbl.ForeColor = Color.FromArgb(60, 60, 60);
        y += 40;
    }

    private static void SetBtn5(Button btn, string text, Point loc, Color color, int width = 110)
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
