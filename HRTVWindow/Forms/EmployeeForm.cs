using HRTVWindow.Data;
using HRTVWindow.Models;

namespace HRTVWindow.Forms;

public partial class EmployeeForm : Form
{
    private readonly EmployeeRepository _repo = new();
    private readonly DepartmentRepository _deptRepo = new();
    private readonly PositionRepository _posRepo = new();
    private Employee? _selected;
    private bool _isNew = false;

    public EmployeeForm()
    {
        InitializeComponent();
    }

    private void EmployeeForm_Load(object sender, EventArgs e)
    {
        LoadDepartments();
        LoadData();
    }

    private void LoadDepartments()
    {
        var depts = _deptRepo.GetAll();
        cboDepartment.DataSource = new List<Department>(depts);
        cboDepartment.DisplayMember = "Name";
        cboDepartment.ValueMember = "Id";
    }

    private void LoadPositions(int departmentId)
    {
        var positions = departmentId > 0 ? _posRepo.GetByDepartment(departmentId) : _posRepo.GetAll();
        cboPosition.DataSource = new List<Position>(positions);
        cboPosition.DisplayMember = "Name";
        cboPosition.ValueMember = "Id";
    }

    private void LoadData(string search = "", int deptFilter = 0, int statusFilter = -1)
    {
        var list = _repo.GetAll(search, deptFilter, statusFilter);
        dgvEmployees.DataSource = list.Select(e => new
        {
            e.Id,
            Mã_NV = e.EmployeeCode,
            Họ_tên = e.FullName,
            Giới_tính = e.Gender,
            Điện_thoại = e.Phone,
            Phòng_ban = e.DepartmentName,
            Chức_vụ = e.PositionName,
            Ngày_vào = e.JoinDate.ToString("dd/MM/yyyy"),
            Trạng_thái = e.StatusText
        }).ToList();

        if (dgvEmployees.Columns.Count > 0)
            dgvEmployees.Columns["Id"].Visible = false;

        lblCount.Text = $"Tổng: {list.Count} nhân viên";
        ClearForm();
    }

    private void dgvEmployees_SelectionChanged(object sender, EventArgs e)
    {
        if (dgvEmployees.CurrentRow == null) return;
        int id = (int)dgvEmployees.CurrentRow.Cells["Id"].Value;
        _selected = _repo.GetById(id);
        if (_selected != null)
        {
            _isNew = false;
            PopulateForm(_selected);
        }
    }

    private void PopulateForm(Employee emp)
    {
        txtCode.Text = emp.EmployeeCode;
        txtFullName.Text = emp.FullName;
        dtpDOB.Value = emp.DateOfBirth;
        cboGender.SelectedItem = emp.Gender;
        txtIDCard.Text = emp.IDCard;
        txtPhone.Text = emp.Phone;
        txtEmail.Text = emp.Email;
        txtAddress.Text = emp.Address;
        dtpJoinDate.Value = emp.JoinDate;
        cboStatus.SelectedIndex = emp.Status == 1 ? 0 : 1;

        cboDepartment.SelectedValue = emp.DepartmentId;
        LoadPositions(emp.DepartmentId);
        cboPosition.SelectedValue = emp.PositionId;
    }

    private void cboDepartment_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cboDepartment.SelectedValue is int deptId)
            LoadPositions(deptId);
    }

    private void btnNew_Click(object sender, EventArgs e)
    {
        _isNew = true;
        _selected = null;
        ClearForm();
        txtCode.Text = _repo.GenerateCode();
        txtFullName.Focus();
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        if (!ValidateForm()) return;

        var emp = new Employee
        {
            EmployeeCode = txtCode.Text.Trim(),
            FullName = txtFullName.Text.Trim(),
            DateOfBirth = dtpDOB.Value.Date,
            Gender = cboGender.SelectedItem?.ToString() ?? "Nam",
            IDCard = txtIDCard.Text.Trim(),
            Phone = txtPhone.Text.Trim(),
            Email = txtEmail.Text.Trim(),
            Address = txtAddress.Text.Trim(),
            DepartmentId = cboDepartment.SelectedValue is int dId ? dId : 0,
            PositionId = cboPosition.SelectedValue is int pId ? pId : 0,
            JoinDate = dtpJoinDate.Value.Date,
            Status = cboStatus.SelectedIndex == 0 ? 1 : 0
        };

        if (_isNew || _selected == null)
        {
            if (_repo.CodeExists(emp.EmployeeCode))
            {
                MessageBox.Show("Mã nhân viên đã tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _repo.Insert(emp);
            MessageBox.Show("Thêm nhân viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            emp.Id = _selected.Id;
            if (_repo.CodeExists(emp.EmployeeCode, emp.Id))
            {
                MessageBox.Show("Mã nhân viên đã tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _repo.Update(emp);
            MessageBox.Show("Cập nhật nhân viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        LoadData(txtSearch.Text, GetFilterDept(), GetFilterStatus());
    }

    private void btnDelete_Click(object sender, EventArgs e)
    {
        if (_selected == null)
        {
            MessageBox.Show("Vui lòng chọn nhân viên cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (MessageBox.Show($"Bạn có chắc muốn xóa nhân viên '{_selected.FullName}'?", "Xác nhận",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            _repo.Delete(_selected.Id);
            LoadData(txtSearch.Text, GetFilterDept(), GetFilterStatus());
            MessageBox.Show("Xóa nhân viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void btnSearch_Click(object sender, EventArgs e)
    {
        LoadData(txtSearch.Text.Trim(), GetFilterDept(), GetFilterStatus());
    }

    private void txtSearch_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
            btnSearch_Click(sender, e);
    }

    private void btnClear_Click(object sender, EventArgs e)
    {
        ClearForm();
        _isNew = false;
        _selected = null;
    }

    private int GetFilterDept()
    {
        return cboFilterDept.SelectedValue is int id ? id : 0;
    }

    private int GetFilterStatus()
    {
        return cboFilterStatus.SelectedIndex switch
        {
            1 => 1,
            2 => 0,
            _ => -1
        };
    }

    private void cboFilterDept_SelectedIndexChanged(object sender, EventArgs e)
    {
        LoadData(txtSearch.Text, GetFilterDept(), GetFilterStatus());
    }

    private void cboFilterStatus_SelectedIndexChanged(object sender, EventArgs e)
    {
        LoadData(txtSearch.Text, GetFilterDept(), GetFilterStatus());
    }

    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(txtCode.Text))
        {
            MessageBox.Show("Vui lòng nhập mã nhân viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        if (string.IsNullOrWhiteSpace(txtFullName.Text))
        {
            MessageBox.Show("Vui lòng nhập họ tên nhân viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        if (cboDepartment.SelectedValue == null)
        {
            MessageBox.Show("Vui lòng chọn phòng ban.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        return true;
    }

    private void ClearForm()
    {
        txtCode.Clear();
        txtFullName.Clear();
        dtpDOB.Value = DateTime.Today.AddYears(-25);
        cboGender.SelectedIndex = 0;
        txtIDCard.Clear();
        txtPhone.Clear();
        txtEmail.Clear();
        txtAddress.Clear();
        dtpJoinDate.Value = DateTime.Today;
        cboStatus.SelectedIndex = 0;
        if (cboDepartment.Items.Count > 0) cboDepartment.SelectedIndex = 0;
    }
}
