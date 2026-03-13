using Microsoft.Data.Sqlite;
using HRTVWindow.Models;

namespace HRTVWindow.Data;

public class DepartmentRepository
{
    public List<Department> GetAll()
    {
        var list = new List<Department>();
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, Description FROM Departments ORDER BY Name";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new Department
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2)
            });
        }
        return list;
    }

    public Department? GetById(int id)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, Description FROM Departments WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new Department
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? string.Empty : reader.GetString(2)
            };
        }
        return null;
    }

    public void Insert(Department dept)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Departments (Name, Description) VALUES (@name, @desc)";
        cmd.Parameters.AddWithValue("@name", dept.Name);
        cmd.Parameters.AddWithValue("@desc", dept.Description);
        cmd.ExecuteNonQuery();
    }

    public void Update(Department dept)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Departments SET Name = @name, Description = @desc WHERE Id = @id";
        cmd.Parameters.AddWithValue("@name", dept.Name);
        cmd.Parameters.AddWithValue("@desc", dept.Description);
        cmd.Parameters.AddWithValue("@id", dept.Id);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Departments WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public bool HasEmployees(int id)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Employees WHERE DepartmentId = @id";
        cmd.Parameters.AddWithValue("@id", id);
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }

    public int GetEmployeeCount(int id)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Employees WHERE DepartmentId = @id AND Status = 1";
        cmd.Parameters.AddWithValue("@id", id);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }
}
