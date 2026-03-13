using HRTVWindow.Data;
using HRTVWindow.Models;

namespace HRTVWindow.Forms;

public partial class SalaryForm : Form
{
    private readonly SalaryRepository _repo = new();
    private readonly EmployeeRepository _empRepo = new();
    private Salary? _selected;

    public SalaryForm()
    {
        InitializeComponent();
    }

    private void SalaryForm_Load(object sender, EventArgs e)
    {
        numMonth.Value = DateTime.Today.Month;
        numYear.Value = DateTime.Today.Year;
        LoadData();
        UpdateNetSalary();
    }

    private void LoadData()
    {
        int month = (int)numMonth.Value;
        int year = (int)numYear.Value;
        var list = _repo.GetAll(month, year);

        dgvSalaries.DataSource = list.Select(s => new
        {
            s.Id,
            Mã_NV = s.EmployeeCode,
            Nhân_viên = s.EmployeeName,
            Phòng_ban = s.DepartmentName,
            Ngày_công_HĐ = s.WorkingDays,
            Ngày_công_TT = s.ActualWorkingDays,
            Lương_cơ_bản = s.BasicSalary.ToString("N0"),
            Phụ_cấp = s.Allowance.ToString("N0"),
            Thưởng = s.Bonus.ToString("N0"),
            Khấu_trừ = s.Deduction.ToString("N0"),
            Thực_lĩnh = s.NetSalary.ToString("N0"),
            Đã_trả = s.IsPaid ? "✓" : ""
        }).ToList();

        if (dgvSalaries.Columns.Count > 0)
            dgvSalaries.Columns["Id"].Visible = false;

        ClearForm();
    }

    private void dgvSalaries_SelectionChanged(object sender, EventArgs e)
    {
        if (dgvSalaries.CurrentRow == null) return;
        int id = (int)dgvSalaries.CurrentRow.Cells["Id"].Value;
        _selected = _repo.GetAll().FirstOrDefault(s => s.Id == id);
        if (_selected != null)
        {
            cboEmployee.SelectedValue = _selected.EmployeeId;
            numWorkDays.Value = _selected.WorkingDays;
            numActualDays.Value = _selected.ActualWorkingDays;
            numBasicSalary.Value = (decimal)_selected.BasicSalary;
            numAllowance.Value = (decimal)_selected.Allowance;
            numBonus.Value = (decimal)_selected.Bonus;
            numDeduction.Value = (decimal)_selected.Deduction;
            txtNotes.Text = _selected.Notes;
            chkIsPaid.Checked = _selected.IsPaid;
            UpdateNetSalary();
        }
    }

    private void UpdateNetSalary()
    {
        decimal workDays = numWorkDays.Value > 0 ? numWorkDays.Value : 1;
        decimal netSalary = numBasicSalary.Value / workDays * numActualDays.Value
            + numAllowance.Value + numBonus.Value - numDeduction.Value;
        lblNetSalary.Text = netSalary.ToString("N0") + " đ";
    }

    private void numericChanged(object sender, EventArgs e) => UpdateNetSalary();

    private void btnSearch_Click(object sender, EventArgs e) => LoadData();

    private void btnSave_Click(object sender, EventArgs e)
    {
        if (cboEmployee.SelectedValue == null)
        {
            MessageBox.Show("Vui lòng chọn nhân viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int empId = (int)cboEmployee.SelectedValue;
        int month = (int)numMonth.Value;
        int year = (int)numYear.Value;

        if (_selected == null && _repo.SalaryExistsForEmployee(empId, month, year))
        {
            MessageBox.Show("Nhân viên này đã có bảng lương tháng này.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var salary = new Salary
        {
            EmployeeId = empId,
            Month = month,
            Year = year,
            WorkingDays = (int)numWorkDays.Value,
            ActualWorkingDays = (int)numActualDays.Value,
            BasicSalary = numBasicSalary.Value,
            Allowance = numAllowance.Value,
            Bonus = numBonus.Value,
            Deduction = numDeduction.Value,
            Notes = txtNotes.Text.Trim(),
            IsPaid = chkIsPaid.Checked,
            PaidDate = chkIsPaid.Checked ? DateTime.Today : null
        };

        if (_selected == null)
        {
            _repo.Insert(salary);
            MessageBox.Show("Thêm bảng lương thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            salary.Id = _selected.Id;
            _repo.Update(salary);
            MessageBox.Show("Cập nhật bảng lương thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        LoadData();
    }

    private void btnDelete_Click(object sender, EventArgs e)
    {
        if (_selected == null)
        {
            MessageBox.Show("Vui lòng chọn bảng lương cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (MessageBox.Show("Bạn có chắc muốn xóa bảng lương này?", "Xác nhận",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            _repo.Delete(_selected.Id);
            LoadData();
            MessageBox.Show("Xóa bảng lương thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void btnClear_Click(object sender, EventArgs e) => ClearForm();

    private void ClearForm()
    {
        _selected = null;
        if (cboEmployee.Items.Count > 0) cboEmployee.SelectedIndex = 0;
        numWorkDays.Value = 26;
        numActualDays.Value = 26;
        numBasicSalary.Value = 0;
        numAllowance.Value = 0;
        numBonus.Value = 0;
        numDeduction.Value = 0;
        txtNotes.Clear();
        chkIsPaid.Checked = false;
        UpdateNetSalary();
    }
}
