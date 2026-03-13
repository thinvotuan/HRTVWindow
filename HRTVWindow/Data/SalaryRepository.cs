using Microsoft.Data.Sqlite;
using HRTVWindow.Models;

namespace HRTVWindow.Data;

public class SalaryRepository
{
    public List<Salary> GetAll(int month = 0, int year = 0, int employeeId = 0)
    {
        var list = new List<Salary>();
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        var where = new List<string>();
        if (month > 0) where.Add("s.Month = @month");
        if (year > 0) where.Add("s.Year = @year");
        if (employeeId > 0) where.Add("s.EmployeeId = @empId");
        var whereClause = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";

        cmd.CommandText = $@"
            SELECT s.Id, s.EmployeeId,
                   COALESCE(e.FullName, '') AS EmpName, COALESCE(e.EmployeeCode, '') AS EmpCode,
                   COALESCE(d.Name, '') AS DeptName,
                   s.Month, s.Year, s.WorkingDays, s.ActualWorkingDays,
                   s.BasicSalary, s.Allowance, s.Bonus, s.Deduction,
                   s.Notes, s.PaidDate, s.IsPaid
            FROM Salaries s
            LEFT JOIN Employees e ON s.EmployeeId = e.Id
            LEFT JOIN Departments d ON e.DepartmentId = d.Id
            {whereClause}
            ORDER BY s.Year DESC, s.Month DESC, e.EmployeeCode";

        if (month > 0) cmd.Parameters.AddWithValue("@month", month);
        if (year > 0) cmd.Parameters.AddWithValue("@year", year);
        if (employeeId > 0) cmd.Parameters.AddWithValue("@empId", employeeId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapSalary(reader));
        }
        return list;
    }

    public void Insert(Salary salary)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Salaries (EmployeeId, Month, Year, WorkingDays, ActualWorkingDays, BasicSalary, Allowance, Bonus, Deduction, Notes, PaidDate, IsPaid)
            VALUES (@empId, @month, @year, @workDays, @actDays, @basic, @allow, @bonus, @deduct, @notes, @paidDate, @isPaid)";
        SetParameters(cmd, salary);
        cmd.ExecuteNonQuery();
    }

    public void Update(Salary salary)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE Salaries SET EmployeeId=@empId, Month=@month, Year=@year,
                WorkingDays=@workDays, ActualWorkingDays=@actDays,
                BasicSalary=@basic, Allowance=@allow, Bonus=@bonus, Deduction=@deduct,
                Notes=@notes, PaidDate=@paidDate, IsPaid=@isPaid
            WHERE Id=@id";
        SetParameters(cmd, salary);
        cmd.Parameters.AddWithValue("@id", salary.Id);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Salaries WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public bool SalaryExistsForEmployee(int employeeId, int month, int year, int excludeId = 0)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Salaries WHERE EmployeeId=@empId AND Month=@month AND Year=@year AND Id!=@excludeId";
        cmd.Parameters.AddWithValue("@empId", employeeId);
        cmd.Parameters.AddWithValue("@month", month);
        cmd.Parameters.AddWithValue("@year", year);
        cmd.Parameters.AddWithValue("@excludeId", excludeId);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    private static void SetParameters(SqliteCommand cmd, Salary salary)
    {
        cmd.Parameters.AddWithValue("@empId", salary.EmployeeId);
        cmd.Parameters.AddWithValue("@month", salary.Month);
        cmd.Parameters.AddWithValue("@year", salary.Year);
        cmd.Parameters.AddWithValue("@workDays", salary.WorkingDays);
        cmd.Parameters.AddWithValue("@actDays", salary.ActualWorkingDays);
        cmd.Parameters.AddWithValue("@basic", salary.BasicSalary);
        cmd.Parameters.AddWithValue("@allow", salary.Allowance);
        cmd.Parameters.AddWithValue("@bonus", salary.Bonus);
        cmd.Parameters.AddWithValue("@deduct", salary.Deduction);
        cmd.Parameters.AddWithValue("@notes", salary.Notes);
        cmd.Parameters.AddWithValue("@paidDate", salary.PaidDate.HasValue ? (object)salary.PaidDate.Value.ToString("yyyy-MM-dd") : DBNull.Value);
        cmd.Parameters.AddWithValue("@isPaid", salary.IsPaid ? 1 : 0);
    }

    private static Salary MapSalary(SqliteDataReader reader)
    {
        return new Salary
        {
            Id = reader.GetInt32(0),
            EmployeeId = reader.GetInt32(1),
            EmployeeName = reader.GetString(2),
            EmployeeCode = reader.GetString(3),
            DepartmentName = reader.GetString(4),
            Month = reader.GetInt32(5),
            Year = reader.GetInt32(6),
            WorkingDays = reader.GetInt32(7),
            ActualWorkingDays = reader.GetInt32(8),
            BasicSalary = (decimal)reader.GetDouble(9),
            Allowance = (decimal)reader.GetDouble(10),
            Bonus = (decimal)reader.GetDouble(11),
            Deduction = (decimal)reader.GetDouble(12),
            Notes = reader.IsDBNull(13) ? string.Empty : reader.GetString(13),
            PaidDate = reader.IsDBNull(14) ? null : DateTime.Parse(reader.GetString(14)),
            IsPaid = reader.GetInt32(15) == 1
        };
    }
}
