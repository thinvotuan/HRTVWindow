using HRTVWindow.Data;
using HRTVWindow.Models;

namespace HRTVWindow.Forms;

public partial class ContractForm : Form
{
    private readonly ContractRepository _repo = new();
    private readonly EmployeeRepository _empRepo = new();
    private Contract? _selected;

    public ContractForm()
    {
        InitializeComponent();
    }

    private void ContractForm_Load(object sender, EventArgs e)
    {
        LoadEmployees();
        LoadData();
    }

    private void LoadEmployees()
    {
        var emps = _empRepo.GetAll(status: 1);
        cboEmployee.DataSource = emps;
        cboEmployee.DisplayMember = "ToString";
        cboEmployee.ValueMember = "Id";
    }

    private void LoadData()
    {
        int empFilter = cboFilterEmployee.SelectedValue is int id && id > 0 ? id : 0;
        var list = _repo.GetAll(empFilter);
        dgvContracts.DataSource = list.Select(c => new
        {
            c.Id,
            Số_HĐ = c.ContractNumber,
            Mã_NV = c.EmployeeCode,
            Nhân_viên = c.EmployeeName,
            Loại_HĐ = c.ContractType,
            Ngày_bắt_đầu = c.StartDate.ToString("dd/MM/yyyy"),
            Ngày_kết_thúc = c.EndDate.HasValue ? c.EndDate.Value.ToString("dd/MM/yyyy") : "Không thời hạn",
            Lương_cơ_bản = c.BasicSalary.ToString("N0")
        }).ToList();

        if (dgvContracts.Columns.Count > 0)
            dgvContracts.Columns["Id"].Visible = false;

        ClearForm();
    }

    private void dgvContracts_SelectionChanged(object sender, EventArgs e)
    {
        if (dgvContracts.CurrentRow == null) return;
        int id = (int)dgvContracts.CurrentRow.Cells["Id"].Value;
        _selected = _repo.GetAll().FirstOrDefault(c => c.Id == id);
        if (_selected != null)
        {
            cboEmployee.SelectedValue = _selected.EmployeeId;
            txtContractNumber.Text = _selected.ContractNumber;
            cboContractType.SelectedItem = _selected.ContractType;
            dtpStartDate.Value = _selected.StartDate;
            chkNoEndDate.Checked = !_selected.EndDate.HasValue;
            if (_selected.EndDate.HasValue) dtpEndDate.Value = _selected.EndDate.Value;
            txtBasicSalary.Text = _selected.BasicSalary.ToString("N0");
            txtNotes.Text = _selected.Notes;
        }
    }

    private void chkNoEndDate_CheckedChanged(object sender, EventArgs e)
    {
        dtpEndDate.Enabled = !chkNoEndDate.Checked;
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        if (!ValidateForm()) return;

        decimal.TryParse(txtBasicSalary.Text.Replace(",", "").Replace(".", ""), out decimal salary);
        var contract = new Contract
        {
            ContractNumber = txtContractNumber.Text.Trim(),
            EmployeeId = cboEmployee.SelectedValue is int eid ? eid : 0,
            ContractType = cboContractType.SelectedItem?.ToString() ?? "",
            StartDate = dtpStartDate.Value.Date,
            EndDate = chkNoEndDate.Checked ? null : dtpEndDate.Value.Date,
            BasicSalary = salary,
            Notes = txtNotes.Text.Trim()
        };

        if (_selected == null)
        {
            _repo.Insert(contract);
            MessageBox.Show("Thêm hợp đồng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            contract.Id = _selected.Id;
            _repo.Update(contract);
            MessageBox.Show("Cập nhật hợp đồng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        LoadData();
    }

    private void btnDelete_Click(object sender, EventArgs e)
    {
        if (_selected == null)
        {
            MessageBox.Show("Vui lòng chọn hợp đồng cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (MessageBox.Show($"Bạn có chắc muốn xóa hợp đồng '{_selected.ContractNumber}'?", "Xác nhận",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            _repo.Delete(_selected.Id);
            LoadData();
            MessageBox.Show("Xóa hợp đồng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void btnClear_Click(object sender, EventArgs e) => ClearForm();

    private void cboFilterEmployee_SelectedIndexChanged(object sender, EventArgs e) => LoadData();

    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(txtContractNumber.Text))
        {
            MessageBox.Show("Vui lòng nhập số hợp đồng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        if (cboEmployee.SelectedValue == null)
        {
            MessageBox.Show("Vui lòng chọn nhân viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        if (!decimal.TryParse(txtBasicSalary.Text.Replace(",", "").Replace(".", ""), out _))
        {
            MessageBox.Show("Lương cơ bản không hợp lệ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        return true;
    }

    private void ClearForm()
    {
        _selected = null;
        txtContractNumber.Clear();
        if (cboEmployee.Items.Count > 0) cboEmployee.SelectedIndex = 0;
        if (cboContractType.Items.Count > 0) cboContractType.SelectedIndex = 0;
        dtpStartDate.Value = DateTime.Today;
        dtpEndDate.Value = DateTime.Today.AddYears(1);
        chkNoEndDate.Checked = false;
        txtBasicSalary.Text = "0";
        txtNotes.Clear();
    }
}
