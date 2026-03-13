namespace HRTVWindow.Models;

public class Salary
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Year { get; set; }
    public int WorkingDays { get; set; }
    public int ActualWorkingDays { get; set; }
    public decimal BasicSalary { get; set; }
    public decimal Allowance { get; set; }
    public decimal Bonus { get; set; }
    public decimal Deduction { get; set; }
    public decimal NetSalary => WorkingDays > 0
        ? BasicSalary / WorkingDays * ActualWorkingDays + Allowance + Bonus - Deduction
        : Allowance + Bonus - Deduction;
    public string Notes { get; set; } = string.Empty;
    public DateTime? PaidDate { get; set; }
    public bool IsPaid { get; set; }
}
