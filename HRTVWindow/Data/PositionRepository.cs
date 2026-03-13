using Microsoft.Data.Sqlite;
using HRTVWindow.Models;

namespace HRTVWindow.Data;

public class PositionRepository
{
    public List<Position> GetAll()
    {
        var list = new List<Position>();
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT p.Id, p.Name, p.Description, p.DepartmentId, COALESCE(d.Name, '') AS DepartmentName
            FROM Positions p
            LEFT JOIN Departments d ON p.DepartmentId = d.Id
            ORDER BY p.Name";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Position
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                DepartmentId = reader.GetInt32(3),
                DepartmentName = reader.GetString(4)
            });
        }
        return list;
    }

    public List<Position> GetByDepartment(int departmentId)
    {
        var list = new List<Position>();
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, Description, DepartmentId FROM Positions WHERE DepartmentId = @deptId ORDER BY Name";
        cmd.Parameters.AddWithValue("@deptId", departmentId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Position
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                DepartmentId = reader.GetInt32(3)
            });
        }
        return list;
    }

    public void Insert(Position pos)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Positions (Name, Description, DepartmentId) VALUES (@name, @desc, @deptId)";
        cmd.Parameters.AddWithValue("@name", pos.Name);
        cmd.Parameters.AddWithValue("@desc", pos.Description);
        cmd.Parameters.AddWithValue("@deptId", pos.DepartmentId);
        cmd.ExecuteNonQuery();
    }

    public void Update(Position pos)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Positions SET Name = @name, Description = @desc, DepartmentId = @deptId WHERE Id = @id";
        cmd.Parameters.AddWithValue("@name", pos.Name);
        cmd.Parameters.AddWithValue("@desc", pos.Description);
        cmd.Parameters.AddWithValue("@deptId", pos.DepartmentId);
        cmd.Parameters.AddWithValue("@id", pos.Id);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Positions WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public bool HasEmployees(int id)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Employees WHERE PositionId = @id";
        cmd.Parameters.AddWithValue("@id", id);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }
}
