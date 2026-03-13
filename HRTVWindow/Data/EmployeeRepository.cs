using Microsoft.Data.Sqlite;
using HRTVWindow.Models;

namespace HRTVWindow.Data;

public class EmployeeRepository
{
    public List<Employee> GetAll(string searchText = "", int departmentId = 0, int status = -1)
    {
        var list = new List<Employee>();
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        var where = new List<string>();
        if (!string.IsNullOrWhiteSpace(searchText))
            where.Add("(e.FullName LIKE @search OR e.EmployeeCode LIKE @search OR e.Phone LIKE @search)");
        if (departmentId > 0)
            where.Add("e.DepartmentId = @deptId");
        if (status >= 0)
            where.Add("e.Status = @status");

        var whereClause = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";
        cmd.CommandText = $@"
            SELECT e.Id, e.EmployeeCode, e.FullName, e.DateOfBirth, e.Gender,
                   e.IDCard, e.Phone, e.Email, e.Address,
                   e.DepartmentId, COALESCE(d.Name, '') AS DeptName,
                   e.PositionId, COALESCE(p.Name, '') AS PosName,
                   e.JoinDate, e.Status
            FROM Employees e
            LEFT JOIN Departments d ON e.DepartmentId = d.Id
            LEFT JOIN Positions p ON e.PositionId = p.Id
            {whereClause}
            ORDER BY e.EmployeeCode";

        if (!string.IsNullOrWhiteSpace(searchText))
            cmd.Parameters.AddWithValue("@search", $"%{searchText}%");
        if (departmentId > 0)
            cmd.Parameters.AddWithValue("@deptId", departmentId);
        if (status >= 0)
            cmd.Parameters.AddWithValue("@status", status);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapEmployee(reader));
        }
        return list;
    }

    public Employee? GetById(int id)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT e.Id, e.EmployeeCode, e.FullName, e.DateOfBirth, e.Gender,
                   e.IDCard, e.Phone, e.Email, e.Address,
                   e.DepartmentId, COALESCE(d.Name, '') AS DeptName,
                   e.PositionId, COALESCE(p.Name, '') AS PosName,
                   e.JoinDate, e.Status
            FROM Employees e
            LEFT JOIN Departments d ON e.DepartmentId = d.Id
            LEFT JOIN Positions p ON e.PositionId = p.Id
            WHERE e.Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
            return MapEmployee(reader);
        return null;
    }

    public string GenerateCode()
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT MAX(CAST(SUBSTR(EmployeeCode, 3) AS INTEGER)) FROM Employees WHERE EmployeeCode LIKE 'NV%'";
        var result = cmd.ExecuteScalar();
        int next = result == DBNull.Value || result == null ? 1 : Convert.ToInt32(result) + 1;
        return $"NV{next:D4}";
    }

    public void Insert(Employee emp)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Employees (EmployeeCode, FullName, DateOfBirth, Gender, IDCard, Phone, Email, Address, DepartmentId, PositionId, JoinDate, Status)
            VALUES (@code, @name, @dob, @gender, @idcard, @phone, @email, @address, @deptId, @posId, @joinDate, @status)";
        SetParameters(cmd, emp);
        cmd.ExecuteNonQuery();
    }

    public void Update(Employee emp)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE Employees SET EmployeeCode=@code, FullName=@name, DateOfBirth=@dob, Gender=@gender,
                IDCard=@idcard, Phone=@phone, Email=@email, Address=@address,
                DepartmentId=@deptId, PositionId=@posId, JoinDate=@joinDate, Status=@status
            WHERE Id=@id";
        SetParameters(cmd, emp);
        cmd.Parameters.AddWithValue("@id", emp.Id);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Employees WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public bool CodeExists(string code, int excludeId = 0)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Employees WHERE EmployeeCode = @code AND Id != @excludeId";
        cmd.Parameters.AddWithValue("@code", code);
        cmd.Parameters.AddWithValue("@excludeId", excludeId);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public int GetTotalActive()
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Employees WHERE Status = 1";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private static void SetParameters(SqliteCommand cmd, Employee emp)
    {
        cmd.Parameters.AddWithValue("@code", emp.EmployeeCode);
        cmd.Parameters.AddWithValue("@name", emp.FullName);
        cmd.Parameters.AddWithValue("@dob", emp.DateOfBirth.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@gender", emp.Gender);
        cmd.Parameters.AddWithValue("@idcard", emp.IDCard);
        cmd.Parameters.AddWithValue("@phone", emp.Phone);
        cmd.Parameters.AddWithValue("@email", emp.Email);
        cmd.Parameters.AddWithValue("@address", emp.Address);
        cmd.Parameters.AddWithValue("@deptId", emp.DepartmentId);
        cmd.Parameters.AddWithValue("@posId", emp.PositionId);
        cmd.Parameters.AddWithValue("@joinDate", emp.JoinDate.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@status", emp.Status);
    }

    private static Employee MapEmployee(SqliteDataReader reader)
    {
        return new Employee
        {
            Id = reader.GetInt32(0),
            EmployeeCode = reader.GetString(1),
            FullName = reader.GetString(2),
            DateOfBirth = DateTime.Parse(reader.GetString(3)),
            Gender = reader.GetString(4),
            IDCard = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
            Phone = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
            Email = reader.IsDBNull(7) ? string.Empty : reader.GetString(7),
            Address = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
            DepartmentId = reader.GetInt32(9),
            DepartmentName = reader.GetString(10),
            PositionId = reader.GetInt32(11),
            PositionName = reader.GetString(12),
            JoinDate = DateTime.Parse(reader.GetString(13)),
            Status = reader.GetInt32(14)
        };
    }
}
