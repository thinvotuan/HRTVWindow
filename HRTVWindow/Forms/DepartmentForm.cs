using HRTVWindow.Data;
using HRTVWindow.Models;

namespace HRTVWindow.Forms;

public partial class DepartmentForm : Form
{
    private readonly DepartmentRepository _repo = new();
    private Department? _selected;

    public DepartmentForm()
    {
        InitializeComponent();
    }

    private void DepartmentForm_Load(object sender, EventArgs e)
    {
        LoadData();
    }

    private void LoadData()
    {
        var list = _repo.GetAll();
        dgvDepartments.DataSource = list.Select(d => new
        {
            d.Id,
            Tên_phòng_ban = d.Name,
            Mô_tả = d.Description,
            Số_NV = _repo.GetEmployeeCount(d.Id)
        }).ToList();

        if (dgvDepartments.Columns.Count > 0)
            dgvDepartments.Columns["Id"].Visible = false;

        ClearForm();
    }

    private void dgvDepartments_SelectionChanged(object sender, EventArgs e)
    {
        if (dgvDepartments.CurrentRow == null) return;
        int id = (int)dgvDepartments.CurrentRow.Cells["Id"].Value;
        _selected = _repo.GetById(id);
        if (_selected != null)
        {
            txtName.Text = _selected.Name;
            txtDescription.Text = _selected.Description;
        }
    }

    private void btnAdd_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("Vui lòng nhập tên phòng ban.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        var dept = new Department { Name = txtName.Text.Trim(), Description = txtDescription.Text.Trim() };
        _repo.Insert(dept);
        LoadData();
        MessageBox.Show("Thêm phòng ban thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void btnEdit_Click(object sender, EventArgs e)
    {
        if (_selected == null)
        {
            MessageBox.Show("Vui lòng chọn phòng ban cần sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("Vui lòng nhập tên phòng ban.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        _selected.Name = txtName.Text.Trim();
        _selected.Description = txtDescription.Text.Trim();
        _repo.Update(_selected);
        LoadData();
        MessageBox.Show("Cập nhật phòng ban thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void btnDelete_Click(object sender, EventArgs e)
    {
        if (_selected == null)
        {
            MessageBox.Show("Vui lòng chọn phòng ban cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (_repo.HasEmployees(_selected.Id))
        {
            MessageBox.Show("Không thể xóa phòng ban đang có nhân viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (MessageBox.Show($"Bạn có chắc muốn xóa phòng ban '{_selected.Name}'?", "Xác nhận",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            _repo.Delete(_selected.Id);
            LoadData();
            MessageBox.Show("Xóa phòng ban thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void btnClear_Click(object sender, EventArgs e)
    {
        _selected = null;
        ClearForm();
    }

    private void ClearForm()
    {
        _selected = null;
        txtName.Clear();
        txtDescription.Clear();
        txtName.Focus();
    }
}
