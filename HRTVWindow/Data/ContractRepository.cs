using Microsoft.Data.Sqlite;
using HRTVWindow.Models;

namespace HRTVWindow.Data;

public class ContractRepository
{
    public List<Contract> GetAll(int employeeId = 0)
    {
        var list = new List<Contract>();
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        var where = employeeId > 0 ? "WHERE c.EmployeeId = @empId" : "";
        cmd.CommandText = $@"
            SELECT c.Id, c.ContractNumber, c.EmployeeId,
                   COALESCE(e.FullName, '') AS EmpName, COALESCE(e.EmployeeCode, '') AS EmpCode,
                   c.ContractType, c.StartDate, c.EndDate, c.BasicSalary, c.Notes
            FROM Contracts c
            LEFT JOIN Employees e ON c.EmployeeId = e.Id
            {where}
            ORDER BY c.StartDate DESC";
        if (employeeId > 0)
            cmd.Parameters.AddWithValue("@empId", employeeId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapContract(reader));
        }
        return list;
    }

    public void Insert(Contract contract)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Contracts (ContractNumber, EmployeeId, ContractType, StartDate, EndDate, BasicSalary, Notes)
            VALUES (@num, @empId, @type, @start, @end, @salary, @notes)";
        SetParameters(cmd, contract);
        cmd.ExecuteNonQuery();
    }

    public void Update(Contract contract)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE Contracts SET ContractNumber=@num, EmployeeId=@empId, ContractType=@type,
                StartDate=@start, EndDate=@end, BasicSalary=@salary, Notes=@notes
            WHERE Id=@id";
        SetParameters(cmd, contract);
        cmd.Parameters.AddWithValue("@id", contract.Id);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Contracts WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    private static void SetParameters(SqliteCommand cmd, Contract contract)
    {
        cmd.Parameters.AddWithValue("@num", contract.ContractNumber);
        cmd.Parameters.AddWithValue("@empId", contract.EmployeeId);
        cmd.Parameters.AddWithValue("@type", contract.ContractType);
        cmd.Parameters.AddWithValue("@start", contract.StartDate.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@end", contract.EndDate.HasValue ? (object)contract.EndDate.Value.ToString("yyyy-MM-dd") : DBNull.Value);
        cmd.Parameters.AddWithValue("@salary", contract.BasicSalary);
        cmd.Parameters.AddWithValue("@notes", contract.Notes);
    }

    private static Contract MapContract(SqliteDataReader reader)
    {
        return new Contract
        {
            Id = reader.GetInt32(0),
            ContractNumber = reader.GetString(1),
            EmployeeId = reader.GetInt32(2),
            EmployeeName = reader.GetString(3),
            EmployeeCode = reader.GetString(4),
            ContractType = reader.GetString(5),
            StartDate = DateTime.Parse(reader.GetString(6)),
            EndDate = reader.IsDBNull(7) ? null : DateTime.Parse(reader.GetString(7)),
            BasicSalary = (decimal)reader.GetDouble(8),
            Notes = reader.IsDBNull(9) ? string.Empty : reader.GetString(9)
        };
    }
}
