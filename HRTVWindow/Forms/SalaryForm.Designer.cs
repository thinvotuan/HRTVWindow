namespace HRTVWindow.Forms;

partial class SalaryForm
{
    private System.ComponentModel.IContainer components = null;

    private Label lblTitle;
    private Panel pnlTop;
    private Panel pnlLeft;
    private Panel pnlRight;
    private DataGridView dgvSalaries;

    private Label lblMonth;
    private NumericUpDown numMonth;
    private Label lblYear;
    private NumericUpDown numYear;
    private Button btnSearch;

    private Label lblEmployee;
    private ComboBox cboEmployee;
    private Label lblWorkDays;
    private NumericUpDown numWorkDays;
    private Label lblActualDays;
    private NumericUpDown numActualDays;
    private Label lblBasicSalary;
    private NumericUpDown numBasicSalary;
    private Label lblAllowance;
    private NumericUpDown numAllowance;
    private Label lblBonus;
    private NumericUpDown numBonus;
    private Label lblDeduction;
    private NumericUpDown numDeduction;
    private Label lblNetSalaryTitle;
    private Label lblNetSalary;
    private Label lblNotes;
    private TextBox txtNotes;
    private CheckBox chkIsPaid;
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
        dgvSalaries = new DataGridView();
        lblMonth = new Label(); numMonth = new NumericUpDown();
        lblYear = new Label(); numYear = new NumericUpDown();
        btnSearch = new Button();
        lblEmployee = new Label(); cboEmployee = new ComboBox();
        lblWorkDays = new Label(); numWorkDays = new NumericUpDown();
        lblActualDays = new Label(); numActualDays = new NumericUpDown();
        lblBasicSalary = new Label(); numBasicSalary = new NumericUpDown();
        lblAllowance = new Label(); numAllowance = new NumericUpDown();
        lblBonus = new Label(); numBonus = new NumericUpDown();
        lblDeduction = new Label(); numDeduction = new NumericUpDown();
        lblNetSalaryTitle = new Label(); lblNetSalary = new Label();
        lblNotes = new Label(); txtNotes = new TextBox();
        chkIsPaid = new CheckBox();
        btnSave = new Button(); btnDelete = new Button(); btnClear = new Button();

        SuspendLayout();

        this.Text = "Quản lý Bảng lương";
        this.Size = new Size(1280, 700);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(240, 244, 248);
        this.Font = new Font("Segoe UI", 9.5f);
        this.Load += SalaryForm_Load;

        // Title
        lblTitle.Text = "💰  BẢNG LƯƠNG NHÂN VIÊN";
        lblTitle.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(0, 102, 204);
        lblTitle.AutoSize = true;
        lblTitle.Location = new Point(20, 15);

        // pnlTop - filters
        pnlTop.BackColor = Color.White;
        pnlTop.Location = new Point(20, 50);
        pnlTop.Size = new Size(1230, 50);

        lblMonth.Text = "Tháng:";
        lblMonth.AutoSize = true;
        lblMonth.Location = new Point(10, 15);
        numMonth.Location = new Point(65, 12);
        numMonth.Size = new Size(60, 28);
        numMonth.Minimum = 1;
        numMonth.Maximum = 12;

        lblYear.Text = "Năm:";
        lblYear.AutoSize = true;
        lblYear.Location = new Point(140, 15);
        numYear.Location = new Point(175, 12);
        numYear.Size = new Size(80, 28);
        numYear.Minimum = 2000;
        numYear.Maximum = 2100;

        SetBtn4(btnSearch, "🔍  Xem", new Point(270, 10), Color.FromArgb(0, 102, 204), 100);
        btnSearch.Click += btnSearch_Click;

        pnlTop.Controls.AddRange(new Control[] { lblMonth, numMonth, lblYear, numYear, btnSearch });

        // pnlLeft - grid
        pnlLeft.BackColor = Color.White;
        pnlLeft.Location = new Point(20, 110);
        pnlLeft.Size = new Size(780, 530);
        pnlLeft.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;

        dgvSalaries.Dock = DockStyle.Fill;
        dgvSalaries.ReadOnly = true;
        dgvSalaries.AllowUserToAddRows = false;
        dgvSalaries.AllowUserToDeleteRows = false;
        dgvSalaries.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvSalaries.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvSalaries.BackgroundColor = Color.White;
        dgvSalaries.BorderStyle = BorderStyle.None;
        dgvSalaries.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 102, 204);
        dgvSalaries.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvSalaries.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        dgvSalaries.EnableHeadersVisualStyles = false;
        dgvSalaries.RowTemplate.Height = 30;
        dgvSalaries.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 249, 255);
        dgvSalaries.SelectionChanged += dgvSalaries_SelectionChanged;
        pnlLeft.Controls.Add(dgvSalaries);

        // pnlRight - form
        pnlRight.BackColor = Color.White;
        pnlRight.Location = new Point(815, 110);
        pnlRight.Size = new Size(435, 530);
        pnlRight.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
        pnlRight.AutoScroll = true;

        int y = 15;

        AddSalaryRow(pnlRight, lblEmployee, "Nhân viên:", ref y);
        cboEmployee.Location = new Point(145, y - 28);
        cboEmployee.Size = new Size(275, 28);
        cboEmployee.DropDownStyle = ComboBoxStyle.DropDownList;
        var emps = new Data.EmployeeRepository().GetAll(status: 1);
        cboEmployee.DataSource = emps;
        cboEmployee.DisplayMember = "ToString";
        cboEmployee.ValueMember = "Id";

        AddSalaryNumRow(pnlRight, lblWorkDays, "Ngày công HĐ:", numWorkDays, ref y, 1, 31, 26);
        AddSalaryNumRow(pnlRight, lblActualDays, "Ngày công TT:", numActualDays, ref y, 0, 31, 26);

        AddSalaryMoneyRow(pnlRight, lblBasicSalary, "Lương cơ bản:", numBasicSalary, ref y);
        numBasicSalary.ValueChanged += numericChanged;
        AddSalaryMoneyRow(pnlRight, lblAllowance, "Phụ cấp:", numAllowance, ref y);
        numAllowance.ValueChanged += numericChanged;
        AddSalaryMoneyRow(pnlRight, lblBonus, "Thưởng:", numBonus, ref y);
        numBonus.ValueChanged += numericChanged;
        AddSalaryMoneyRow(pnlRight, lblDeduction, "Khấu trừ:", numDeduction, ref y);
        numDeduction.ValueChanged += numericChanged;
        numActualDays.ValueChanged += numericChanged;
        numWorkDays.ValueChanged += numericChanged;

        // Net salary display
        lblNetSalaryTitle.Text = "Thực lĩnh:";
        lblNetSalaryTitle.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
        lblNetSalaryTitle.AutoSize = true;
        lblNetSalaryTitle.Location = new Point(15, y + 5);
        lblNetSalaryTitle.ForeColor = Color.FromArgb(0, 128, 0);

        lblNetSalary.Text = "0 đ";
        lblNetSalary.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
        lblNetSalary.AutoSize = true;
        lblNetSalary.Location = new Point(145, y + 2);
        lblNetSalary.ForeColor = Color.FromArgb(0, 128, 0);
        y += 40;

        AddSalaryRow(pnlRight, lblNotes, "Ghi chú:", ref y);
        txtNotes.Location = new Point(145, y - 28);
        txtNotes.Size = new Size(275, 55);
        txtNotes.Font = new Font("Segoe UI", 10f);
        txtNotes.Multiline = true;
        txtNotes.BorderStyle = BorderStyle.FixedSingle;
        y += 30;

        chkIsPaid.Text = "Đã thanh toán";
        chkIsPaid.Location = new Point(145, y);
        chkIsPaid.AutoSize = true;
        y += 40;

        SetBtn4(btnSave, "💾  Lưu", new Point(15, y), Color.FromArgb(0, 128, 0), 120);
        btnSave.Click += btnSave_Click;
        SetBtn4(btnDelete, "🗑️  Xóa", new Point(145, y), Color.FromArgb(180, 0, 0), 120);
        btnDelete.Click += btnDelete_Click;
        SetBtn4(btnClear, "🔄  Mới", new Point(275, y), Color.FromArgb(100, 100, 100), 120);
        btnClear.Click += btnClear_Click;

        pnlRight.Controls.AddRange(new Control[] {
            lblEmployee, cboEmployee,
            lblWorkDays, numWorkDays, lblActualDays, numActualDays,
            lblBasicSalary, numBasicSalary, lblAllowance, numAllowance,
            lblBonus, numBonus, lblDeduction, numDeduction,
            lblNetSalaryTitle, lblNetSalary,
            lblNotes, txtNotes, chkIsPaid,
            btnSave, btnDelete, btnClear
        });

        this.Controls.AddRange(new Control[] { lblTitle, pnlTop, pnlLeft, pnlRight });

        ResumeLayout(false);
        PerformLayout();
    }

    private static void AddSalaryRow(Panel p, Label lbl, string text, ref int y)
    {
        lbl.Text = text;
        lbl.AutoSize = true;
        lbl.Location = new Point(15, y + 5);
        lbl.ForeColor = Color.FromArgb(60, 60, 60);
        y += 40;
    }

    private static void AddSalaryNumRow(Panel p, Label lbl, string text, NumericUpDown num, ref int y, int min, int max, int val)
    {
        lbl.Text = text;
        lbl.AutoSize = true;
        lbl.Location = new Point(15, y + 5);
        lbl.ForeColor = Color.FromArgb(60, 60, 60);
        num.Location = new Point(145, y);
        num.Size = new Size(100, 28);
        num.Minimum = min;
        num.Maximum = max;
        num.Value = val;
        y += 40;
    }

    private static void AddSalaryMoneyRow(Panel p, Label lbl, string text, NumericUpDown num, ref int y)
    {
        lbl.Text = text;
        lbl.AutoSize = true;
        lbl.Location = new Point(15, y + 5);
        lbl.ForeColor = Color.FromArgb(60, 60, 60);
        num.Location = new Point(145, y);
        num.Size = new Size(190, 28);
        num.Minimum = 0;
        num.Maximum = 999_000_000;
        num.Increment = 100000;
        num.ThousandsSeparator = true;
        y += 40;
    }

    private static void SetBtn4(Button btn, string text, Point loc, Color color, int width = 120)
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
