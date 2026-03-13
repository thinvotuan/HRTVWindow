namespace HRTVWindow.Forms;

partial class DepartmentForm
{
    private System.ComponentModel.IContainer components = null;

    private Label lblTitle;
    private DataGridView dgvDepartments;
    private Panel pnlForm;
    private Label lblName;
    private TextBox txtName;
    private Label lblDescription;
    private TextBox txtDescription;
    private Button btnAdd;
    private Button btnEdit;
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
        dgvDepartments = new DataGridView();
        pnlForm = new Panel();
        lblName = new Label();
        txtName = new TextBox();
        lblDescription = new Label();
        txtDescription = new TextBox();
        btnAdd = new Button();
        btnEdit = new Button();
        btnDelete = new Button();
        btnClear = new Button();

        SuspendLayout();

        // Form
        this.Text = "Quản lý Phòng ban";
        this.Size = new Size(900, 580);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(240, 244, 248);
        this.Font = new Font("Segoe UI", 9.5f);
        this.Load += DepartmentForm_Load;

        // lblTitle
        lblTitle.Text = "🏬  QUẢN LÝ PHÒNG BAN";
        lblTitle.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(0, 102, 204);
        lblTitle.AutoSize = true;
        lblTitle.Location = new Point(20, 15);

        // dgvDepartments
        dgvDepartments.Location = new Point(20, 55);
        dgvDepartments.Size = new Size(540, 450);
        dgvDepartments.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
        dgvDepartments.ReadOnly = true;
        dgvDepartments.AllowUserToAddRows = false;
        dgvDepartments.AllowUserToDeleteRows = false;
        dgvDepartments.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvDepartments.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvDepartments.BackgroundColor = Color.White;
        dgvDepartments.BorderStyle = BorderStyle.None;
        dgvDepartments.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 102, 204);
        dgvDepartments.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvDepartments.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        dgvDepartments.EnableHeadersVisualStyles = false;
        dgvDepartments.RowTemplate.Height = 30;
        dgvDepartments.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 249, 255);
        dgvDepartments.SelectionChanged += dgvDepartments_SelectionChanged;

        // pnlForm
        pnlForm.BackColor = Color.White;
        pnlForm.Location = new Point(580, 55);
        pnlForm.Size = new Size(290, 450);
        pnlForm.Padding = new Padding(15);

        // Form fields
        AddLabel(pnlForm, lblName, "Tên phòng ban: *", new Point(15, 15));
        txtName.Location = new Point(15, 38);
        txtName.Size = new Size(255, 28);
        txtName.Font = new Font("Segoe UI", 10f);
        txtName.BorderStyle = BorderStyle.FixedSingle;

        AddLabel(pnlForm, lblDescription, "Mô tả:", new Point(15, 75));
        txtDescription.Location = new Point(15, 98);
        txtDescription.Size = new Size(255, 80);
        txtDescription.Font = new Font("Segoe UI", 10f);
        txtDescription.Multiline = true;
        txtDescription.BorderStyle = BorderStyle.FixedSingle;
        txtDescription.ScrollBars = ScrollBars.Vertical;

        ConfigureButton(btnAdd, "➕  Thêm mới", new Point(15, 200), Color.FromArgb(0, 128, 0));
        btnAdd.Click += btnAdd_Click;
        ConfigureButton(btnEdit, "✏️  Cập nhật", new Point(15, 252), Color.FromArgb(0, 102, 204));
        btnEdit.Click += btnEdit_Click;
        ConfigureButton(btnDelete, "🗑️  Xóa", new Point(15, 304), Color.FromArgb(180, 0, 0));
        btnDelete.Click += btnDelete_Click;
        ConfigureButton(btnClear, "🔄  Làm mới", new Point(15, 356), Color.FromArgb(100, 100, 100));
        btnClear.Click += btnClear_Click;

        pnlForm.Controls.AddRange(new Control[] { lblName, txtName, lblDescription, txtDescription, btnAdd, btnEdit, btnDelete, btnClear });

        this.Controls.AddRange(new Control[] { lblTitle, dgvDepartments, pnlForm });

        ResumeLayout(false);
        PerformLayout();
    }

    private static void AddLabel(Panel parent, Label lbl, string text, Point location)
    {
        lbl.Text = text;
        lbl.AutoSize = true;
        lbl.Location = location;
        lbl.ForeColor = Color.FromArgb(60, 60, 60);
        lbl.Font = new Font("Segoe UI", 9.5f);
    }

    private static void ConfigureButton(Button btn, string text, Point location, Color color)
    {
        btn.Text = text;
        btn.Location = location;
        btn.Size = new Size(255, 42);
        btn.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
        btn.BackColor = color;
        btn.ForeColor = Color.White;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.Cursor = Cursors.Hand;
    }
}
