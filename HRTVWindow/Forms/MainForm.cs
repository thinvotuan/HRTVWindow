using HRTVWindow.Data;
using HRTVWindow.Models;

namespace HRTVWindow.Forms;

public partial class MainForm : Form
{
    private readonly User _currentUser;
    private readonly EmployeeRepository _empRepo = new();
    private readonly DepartmentRepository _deptRepo = new();

    public MainForm(User user)
    {
        _currentUser = user;
        InitializeComponent();
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        lblWelcome.Text = $"Xin chào, {_currentUser.FullName}  |  Vai trò: {_currentUser.Role}";
        lblDateTime.Text = DateTime.Now.ToString("dddd, dd/MM/yyyy HH:mm");
        LoadDashboard();

        timerClock.Start();
    }

    private void LoadDashboard()
    {
        try
        {
            int totalEmployees = _empRepo.GetTotalActive();
            var departments = _deptRepo.GetAll();

            lblTotalEmployees.Text = totalEmployees.ToString();
            lblTotalDepartments.Text = departments.Count.ToString();
            lblCurrentDate.Text = DateTime.Now.ToString("dd/MM/yyyy");

            // Load recent employees
            var recentEmps = _empRepo.GetAll();
            dgvDashboard.DataSource = recentEmps.Take(10).Select(e => new
            {
                Mã_NV = e.EmployeeCode,
                Họ_tên = e.FullName,
                Phòng_ban = e.DepartmentName,
                Chức_vụ = e.PositionName,
                Trạng_thái = e.StatusText
            }).ToList();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void timerClock_Tick(object sender, EventArgs e)
    {
        lblDateTime.Text = DateTime.Now.ToString("dddd, dd/MM/yyyy HH:mm:ss");
    }

    // Navigation
    private void btnEmployees_Click(object sender, EventArgs e)
    {
        var form = new EmployeeForm();
        form.ShowDialog(this);
        LoadDashboard();
    }

    private void btnDepartments_Click(object sender, EventArgs e)
    {
        var form = new DepartmentForm();
        form.ShowDialog(this);
        LoadDashboard();
    }

    private void btnPositions_Click(object sender, EventArgs e)
    {
        var form = new PositionForm();
        form.ShowDialog(this);
    }

    private void btnContracts_Click(object sender, EventArgs e)
    {
        var form = new ContractForm();
        form.ShowDialog(this);
    }

    private void btnSalaries_Click(object sender, EventArgs e)
    {
        var form = new SalaryForm();
        form.ShowDialog(this);
    }

    private void btnAttendance_Click(object sender, EventArgs e)
    {
        var form = new AttendanceForm();
        form.ShowDialog(this);
    }

    private void btnRefresh_Click(object sender, EventArgs e)
    {
        LoadDashboard();
    }

    private void btnLogout_Click(object sender, EventArgs e)
    {
        if (MessageBox.Show("Bạn có chắc muốn đăng xuất?", "Xác nhận",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            Close();
        }
    }
}
