using HRTVWindow.Data;
using HRTVWindow.Models;

namespace HRTVWindow.Forms;

public partial class PositionForm : Form
{
    private readonly PositionRepository _repo = new();
    private readonly DepartmentRepository _deptRepo = new();
    private Position? _selected;

    public PositionForm()
    {
        InitializeComponent();
    }

    private void PositionForm_Load(object sender, EventArgs e)
    {
        LoadDepartments();
        LoadData();
    }

    private void LoadDepartments()
    {
        var depts = _deptRepo.GetAll();
        cboDepartment.DataSource = depts;
        cboDepartment.DisplayMember = "Name";
        cboDepartment.ValueMember = "Id";
    }

    private void LoadData()
    {
        var list = _repo.GetAll();
        dgvPositions.DataSource = list.Select(p => new
        {
            p.Id,
            Tên_chức_vụ = p.Name,
            Phòng_ban = p.DepartmentName,
            Mô_tả = p.Description
        }).ToList();

        if (dgvPositions.Columns.Count > 0)
            dgvPositions.Columns["Id"].Visible = false;

        ClearForm();
    }

    private void dgvPositions_SelectionChanged(object sender, EventArgs e)
    {
        if (dgvPositions.CurrentRow == null) return;
        int id = (int)dgvPositions.CurrentRow.Cells["Id"].Value;
        var pos = _repo.GetAll().FirstOrDefault(p => p.Id == id);
        if (pos != null)
        {
            _selected = pos;
            txtName.Text = pos.Name;
            txtDescription.Text = pos.Description;
            cboDepartment.SelectedValue = pos.DepartmentId;
        }
    }

    private void btnAdd_Click(object sender, EventArgs e)
    {
        if (!ValidateForm()) return;
        var pos = new Position
        {
            Name = txtName.Text.Trim(),
            Description = txtDescription.Text.Trim(),
            DepartmentId = (int)cboDepartment.SelectedValue!
        };
        _repo.Insert(pos);
        LoadData();
        MessageBox.Show("Thêm chức vụ thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void btnEdit_Click(object sender, EventArgs e)
    {
        if (_selected == null)
        {
            MessageBox.Show("Vui lòng chọn chức vụ cần sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (!ValidateForm()) return;
        _selected.Name = txtName.Text.Trim();
        _selected.Description = txtDescription.Text.Trim();
        _selected.DepartmentId = (int)cboDepartment.SelectedValue!;
        _repo.Update(_selected);
        LoadData();
        MessageBox.Show("Cập nhật chức vụ thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void btnDelete_Click(object sender, EventArgs e)
    {
        if (_selected == null)
        {
            MessageBox.Show("Vui lòng chọn chức vụ cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (_repo.HasEmployees(_selected.Id))
        {
            MessageBox.Show("Không thể xóa chức vụ đang có nhân viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (MessageBox.Show($"Bạn có chắc muốn xóa chức vụ '{_selected.Name}'?", "Xác nhận",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            _repo.Delete(_selected.Id);
            LoadData();
            MessageBox.Show("Xóa chức vụ thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void btnClear_Click(object sender, EventArgs e) => ClearForm();

    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("Vui lòng nhập tên chức vụ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        _selected = null;
        txtName.Clear();
        txtDescription.Clear();
        if (cboDepartment.Items.Count > 0) cboDepartment.SelectedIndex = 0;
        txtName.Focus();
    }
}
