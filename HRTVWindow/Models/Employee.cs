namespace HRTVWindow.Models;

public class Employee
{
    public int Id { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = "Nam";
    public string IDCard { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int PositionId { get; set; }
    public string PositionName { get; set; } = string.Empty;
    public DateTime JoinDate { get; set; }
    public int Status { get; set; } = 1; // 1=Active, 0=Resigned
    public string StatusText => Status == 1 ? "Đang làm việc" : "Đã nghỉ việc";

    public override string ToString() => $"{EmployeeCode} - {FullName}";
}
