using Microsoft.Data.Sqlite;
using HRTVWindow.Models;

namespace HRTVWindow.Data;

public class UserRepository
{
    public User? Login(string username, string password)
    {
        using var conn = DatabaseHelper.GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Username, PasswordHash, FullName, Role, IsActive FROM Users WHERE Username = @username AND IsActive = 1";
        cmd.Parameters.AddWithValue("@username", username);
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            var hash = reader.GetString(2);
            if (DatabaseHelper.VerifyPassword(password, hash))
            {
                return new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    PasswordHash = hash,
                    FullName = reader.GetString(3),
                    Role = reader.GetString(4),
                    IsActive = reader.GetInt32(5) == 1
                };
            }
        }
        return null;
    }
}
