namespace HRTVWindow.Models;

public class Attendance
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string CheckIn { get; set; } = string.Empty;
    public string CheckOut { get; set; } = string.Empty;
    public string Status { get; set; } = "Có mặt"; // Có mặt, Đi trễ, Vắng mặt, Nghỉ phép
    public string Notes { get; set; } = string.Empty;
}
