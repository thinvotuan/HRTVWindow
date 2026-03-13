using Microsoft.Data.Sqlite;
using HRTVWindow.Models;

namespace HRTVWindow.Data;

public class AttendanceRepository
{
    public List<Attendance> GetAll(DateTime? date = null, int employeeId = 0, int month = 0, int year = 0)
    {
        var list = new List<Attendance>();
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        var where = new List<string>();
        if (date.HasValue)
            where.Add("a.Date = @date");
        if (employeeId > 0)
            where.Add("a.EmployeeId = @empId");
        if (month > 0)
            where.Add("CAST(STRFTIME('%m', a.Date) AS INTEGER) = @month");
        if (year > 0)
            where.Add("CAST(STRFTIME('%Y', a.Date) AS INTEGER) = @year");
        var whereClause = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : "";

        cmd.CommandText = $@"
            SELECT a.Id, a.EmployeeId,
                   COALESCE(e.FullName, '') AS EmpName, COALESCE(e.EmployeeCode, '') AS EmpCode,
                   a.Date, a.CheckIn, a.CheckOut, a.Status, a.Notes
            FROM Attendances a
            LEFT JOIN Employees e ON a.EmployeeId = e.Id
            {whereClause}
            ORDER BY a.Date DESC, e.EmployeeCode";

        if (date.HasValue) cmd.Parameters.AddWithValue("@date", date.Value.ToString("yyyy-MM-dd"));
        if (employeeId > 0) cmd.Parameters.AddWithValue("@empId", employeeId);
        if (month > 0) cmd.Parameters.AddWithValue("@month", month);
        if (year > 0) cmd.Parameters.AddWithValue("@year", year);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(MapAttendance(reader));
        }
        return list;
    }

    public void Insert(Attendance att)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Attendances (EmployeeId, Date, CheckIn, CheckOut, Status, Notes)
            VALUES (@empId, @date, @in, @out, @status, @notes)";
        SetParameters(cmd, att);
        cmd.ExecuteNonQuery();
    }

    public void Update(Attendance att)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE Attendances SET EmployeeId=@empId, Date=@date, CheckIn=@in,
                CheckOut=@out, Status=@status, Notes=@notes
            WHERE Id=@id";
        SetParameters(cmd, att);
        cmd.Parameters.AddWithValue("@id", att.Id);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Attendances WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }

    public int GetCountByStatus(int month, int year, int employeeId, string status)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT COUNT(*) FROM Attendances
            WHERE CAST(STRFTIME('%m', Date) AS INTEGER) = @month
              AND CAST(STRFTIME('%Y', Date) AS INTEGER) = @year
              AND EmployeeId = @empId
              AND Status = @status";
        cmd.Parameters.AddWithValue("@month", month);
        cmd.Parameters.AddWithValue("@year", year);
        cmd.Parameters.AddWithValue("@empId", employeeId);
        cmd.Parameters.AddWithValue("@status", status);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private static void SetParameters(SqliteCommand cmd, Attendance att)
    {
        cmd.Parameters.AddWithValue("@empId", att.EmployeeId);
        cmd.Parameters.AddWithValue("@date", att.Date.ToString("yyyy-MM-dd"));
        cmd.Parameters.AddWithValue("@in", att.CheckIn);
        cmd.Parameters.AddWithValue("@out", att.CheckOut);
        cmd.Parameters.AddWithValue("@status", att.Status);
        cmd.Parameters.AddWithValue("@notes", att.Notes);
    }

    private static Attendance MapAttendance(SqliteDataReader reader)
    {
        return new Attendance
        {
            Id = reader.GetInt32(0),
            EmployeeId = reader.GetInt32(1),
            EmployeeName = reader.GetString(2),
            EmployeeCode = reader.GetString(3),
            Date = DateTime.Parse(reader.GetString(4)),
            CheckIn = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
            CheckOut = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
            Status = reader.GetString(7),
            Notes = reader.IsDBNull(8) ? string.Empty : reader.GetString(8)
        };
    }
}
