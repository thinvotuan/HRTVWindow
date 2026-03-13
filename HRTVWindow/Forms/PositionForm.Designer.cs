namespace HRTVWindow.Forms;

partial class PositionForm
{
    private System.ComponentModel.IContainer components = null;

    private Label lblTitle;
    private DataGridView dgvPositions;
    private Panel pnlForm;
    private Label lblName;
    private TextBox txtName;
    private Label lblDepartment;
    private ComboBox cboDepartment;
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
        dgvPositions = new DataGridView();
        pnlForm = new Panel();
        lblName = new Label();
        txtName = new TextBox();
        lblDepartment = new Label();
        cboDepartment = new ComboBox();
        lblDescription = new Label();
        txtDescription = new TextBox();
        btnAdd = new Button();
        btnEdit = new Button();
        btnDelete = new Button();
        btnClear = new Button();

        SuspendLayout();

        // Form
        this.Text = "Quản lý Chức vụ";
        this.Size = new Size(950, 600);
        this.StartPosition = FormStartPosition.CenterParent;
        this.BackColor = Color.FromArgb(240, 244, 248);
        this.Font = new Font("Segoe UI", 9.5f);
        this.Load += PositionForm_Load;

        // lblTitle
        lblTitle.Text = "💼  QUẢN LÝ CHỨC VỤ";
        lblTitle.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
        lblTitle.ForeColor = Color.FromArgb(0, 102, 204);
        lblTitle.AutoSize = true;
        lblTitle.Location = new Point(20, 15);

        // dgvPositions
        dgvPositions.Location = new Point(20, 55);
        dgvPositions.Size = new Size(570, 470);
        dgvPositions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
        dgvPositions.ReadOnly = true;
        dgvPositions.AllowUserToAddRows = false;
        dgvPositions.AllowUserToDeleteRows = false;
        dgvPositions.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvPositions.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvPositions.BackgroundColor = Color.White;
        dgvPositions.BorderStyle = BorderStyle.None;
        dgvPositions.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 102, 204);
        dgvPositions.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dgvPositions.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        dgvPositions.EnableHeadersVisualStyles = false;
        dgvPositions.RowTemplate.Height = 30;
        dgvPositions.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 249, 255);
        dgvPositions.SelectionChanged += dgvPositions_SelectionChanged;

        // pnlForm
        pnlForm.BackColor = Color.White;
        pnlForm.Location = new Point(610, 55);
        pnlForm.Size = new Size(310, 470);

        // Name
        SetLabel(lblName, "Tên chức vụ: *", new Point(15, 15));
        txtName.Location = new Point(15, 38);
        txtName.Size = new Size(275, 28);
        txtName.Font = new Font("Segoe UI", 10f);
        txtName.BorderStyle = BorderStyle.FixedSingle;

        // Department
        SetLabel(lblDepartment, "Phòng ban: *", new Point(15, 75));
        cboDepartment.Location = new Point(15, 98);
        cboDepartment.Size = new Size(275, 28);
        cboDepartment.Font = new Font("Segoe UI", 10f);
        cboDepartment.DropDownStyle = ComboBoxStyle.DropDownList;

        // Description
        SetLabel(lblDescription, "Mô tả:", new Point(15, 135));
        txtDescription.Location = new Point(15, 158);
        txtDescription.Size = new Size(275, 70);
        txtDescription.Font = new Font("Segoe UI", 10f);
        txtDescription.Multiline = true;
        txtDescription.BorderStyle = BorderStyle.FixedSingle;

        SetBtn(btnAdd, "➕  Thêm mới", new Point(15, 248), Color.FromArgb(0, 128, 0));
        btnAdd.Click += btnAdd_Click;
        SetBtn(btnEdit, "✏️  Cập nhật", new Point(15, 300), Color.FromArgb(0, 102, 204));
        btnEdit.Click += btnEdit_Click;
        SetBtn(btnDelete, "🗑️  Xóa", new Point(15, 352), Color.FromArgb(180, 0, 0));
        btnDelete.Click += btnDelete_Click;
        SetBtn(btnClear, "🔄  Làm mới", new Point(15, 404), Color.FromArgb(100, 100, 100));
        btnClear.Click += btnClear_Click;

        pnlForm.Controls.AddRange(new Control[] { lblName, txtName, lblDepartment, cboDepartment, lblDescription, txtDescription, btnAdd, btnEdit, btnDelete, btnClear });

        this.Controls.AddRange(new Control[] { lblTitle, dgvPositions, pnlForm });

        ResumeLayout(false);
        PerformLayout();
    }

    private static void SetLabel(Label lbl, string text, Point loc)
    {
        lbl.Text = text;
        lbl.AutoSize = true;
        lbl.Location = loc;
        lbl.ForeColor = Color.FromArgb(60, 60, 60);
    }

    private static void SetBtn(Button btn, string text, Point loc, Color color)
    {
        btn.Text = text;
        btn.Location = loc;
        btn.Size = new Size(275, 42);
        btn.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
        btn.BackColor = color;
        btn.ForeColor = Color.White;
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.Cursor = Cursors.Hand;
    }
}
