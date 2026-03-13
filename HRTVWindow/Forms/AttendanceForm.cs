using HRTVWindow.Data;
using HRTVWindow.Models;

namespace HRTVWindow.Forms;

public partial class AttendanceForm : Form
{
    private readonly AttendanceRepository _repo = new();
    private readonly EmployeeRepository _empRepo = new();
    private Attendance? _selected;

    public AttendanceForm()
    {
        InitializeComponent();
    }

    private void AttendanceForm_Load(object sender, EventArgs e)
    {
        dtpFilterDate.Value = DateTime.Today;
        LoadData();
    }

    private void LoadData()
    {
        DateTime? dateFilter = chkFilterByDate.Checked ? dtpFilterDate.Value.Date : (DateTime?)null;
        int empFilter = cboFilterEmployee.SelectedValue is int id && id > 0 ? id : 0;
        int monthFilter = chkFilterByMonth.Checked ? (int)numFilterMonth.Value : 0;
        int yearFilter = chkFilterByMonth.Checked ? (int)numFilterYear.Value : 0;

        var list = _repo.GetAll(dateFilter, empFilter, monthFilter, yearFilter);
        dgvAttendance.DataSource = list.Select(a => new
        {
            a.Id,
            Mã_NV = a.EmployeeCode,
            Nhân_viên = a.EmployeeName,
            Ngày = a.Date.ToString("dd/MM/yyyy"),
            Vào = a.CheckIn,
            Ra = a.CheckOut,
            Trạng_thái = a.Status,
            Ghi_chú = a.Notes
        }).ToList();

        if (dgvAttendance.Columns.Count > 0)
            dgvAttendance.Columns["Id"].Visible = false;

        lblCount.Text = $"Tổng: {list.Count} bản ghi";
        ClearForm();
    }

    private void dgvAttendance_SelectionChanged(object sender, EventArgs e)
    {
        if (dgvAttendance.CurrentRow == null) return;
        int id = (int)dgvAttendance.CurrentRow.Cells["Id"].Value;
        _selected = _repo.GetAll().FirstOrDefault(a => a.Id == id);
        if (_selected != null)
        {
            cboEmployee.SelectedValue = _selected.EmployeeId;
            dtpDate.Value = _selected.Date;
            txtCheckIn.Text = _selected.CheckIn;
            txtCheckOut.Text = _selected.CheckOut;
            cboStatus.SelectedItem = _selected.Status;
            txtNotes.Text = _selected.Notes;
        }
    }

    private void btnSearch_Click(object sender, EventArgs e) => LoadData();

    private void chkFilterByDate_CheckedChanged(object sender, EventArgs e)
    {
        dtpFilterDate.Enabled = chkFilterByDate.Checked;
        if (chkFilterByDate.Checked) chkFilterByMonth.Checked = false;
    }

    private void chkFilterByMonth_CheckedChanged(object sender, EventArgs e)
    {
        numFilterMonth.Enabled = chkFilterByMonth.Checked;
        numFilterYear.Enabled = chkFilterByMonth.Checked;
        if (chkFilterByMonth.Checked) chkFilterByDate.Checked = false;
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        if (cboEmployee.SelectedValue == null)
        {
            MessageBox.Show("Vui lòng chọn nhân viên.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var att = new Attendance
        {
            EmployeeId = (int)cboEmployee.SelectedValue,
            Date = dtpDate.Value.Date,
            CheckIn = txtCheckIn.Text.Trim(),
            CheckOut = txtCheckOut.Text.Trim(),
            Status = cboStatus.SelectedItem?.ToString() ?? "Có mặt",
            Notes = txtNotes.Text.Trim()
        };

        if (_selected == null)
        {
            _repo.Insert(att);
            MessageBox.Show("Thêm chấm công thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            att.Id = _selected.Id;
            _repo.Update(att);
            MessageBox.Show("Cập nhật chấm công thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        LoadData();
    }

    private void btnDelete_Click(object sender, EventArgs e)
    {
        if (_selected == null)
        {
            MessageBox.Show("Vui lòng chọn bản ghi cần xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (MessageBox.Show("Bạn có chắc muốn xóa bản ghi chấm công này?", "Xác nhận",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        {
            _repo.Delete(_selected.Id);
            LoadData();
            MessageBox.Show("Xóa chấm công thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void btnClear_Click(object sender, EventArgs e) => ClearForm();

    private void ClearForm()
    {
        _selected = null;
        if (cboEmployee.Items.Count > 0) cboEmployee.SelectedIndex = 0;
        dtpDate.Value = DateTime.Today;
        txtCheckIn.Text = "08:00";
        txtCheckOut.Text = "17:30";
        cboStatus.SelectedIndex = 0;
        txtNotes.Clear();
    }
}
