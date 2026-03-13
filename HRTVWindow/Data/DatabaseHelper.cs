using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;

namespace HRTVWindow.Data;

public static class DatabaseHelper
{
    private static string _dbPath = string.Empty;

    public static string ConnectionString { get; private set; } = string.Empty;

    public static void Initialize(string dbPath)
    {
        _dbPath = dbPath;
        ConnectionString = $"Data Source={_dbPath}";
        CreateTables();
        SeedDefaultData();
    }

    public static SqliteConnection GetConnection()
    {
        return new SqliteConnection(ConnectionString);
    }

    private static void CreateTables()
    {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Departments (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT DEFAULT ''
            );

            CREATE TABLE IF NOT EXISTS Positions (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Description TEXT DEFAULT '',
                DepartmentId INTEGER NOT NULL DEFAULT 0,
                FOREIGN KEY (DepartmentId) REFERENCES Departments(Id)
            );

            CREATE TABLE IF NOT EXISTS Employees (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                EmployeeCode TEXT NOT NULL UNIQUE,
                FullName TEXT NOT NULL,
                DateOfBirth TEXT NOT NULL,
                Gender TEXT NOT NULL DEFAULT 'Nam',
                IDCard TEXT DEFAULT '',
                Phone TEXT DEFAULT '',
                Email TEXT DEFAULT '',
                Address TEXT DEFAULT '',
                DepartmentId INTEGER NOT NULL DEFAULT 0,
                PositionId INTEGER NOT NULL DEFAULT 0,
                JoinDate TEXT NOT NULL,
                Status INTEGER NOT NULL DEFAULT 1,
                FOREIGN KEY (DepartmentId) REFERENCES Departments(Id),
                FOREIGN KEY (PositionId) REFERENCES Positions(Id)
            );

            CREATE TABLE IF NOT EXISTS Contracts (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ContractNumber TEXT NOT NULL,
                EmployeeId INTEGER NOT NULL,
                ContractType TEXT NOT NULL DEFAULT '',
                StartDate TEXT NOT NULL,
                EndDate TEXT,
                BasicSalary REAL NOT NULL DEFAULT 0,
                Notes TEXT DEFAULT '',
                FOREIGN KEY (EmployeeId) REFERENCES Employees(Id)
            );

            CREATE TABLE IF NOT EXISTS Salaries (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                EmployeeId INTEGER NOT NULL,
                Month INTEGER NOT NULL,
                Year INTEGER NOT NULL,
                WorkingDays INTEGER NOT NULL DEFAULT 26,
                ActualWorkingDays INTEGER NOT NULL DEFAULT 26,
                BasicSalary REAL NOT NULL DEFAULT 0,
                Allowance REAL NOT NULL DEFAULT 0,
                Bonus REAL NOT NULL DEFAULT 0,
                Deduction REAL NOT NULL DEFAULT 0,
                Notes TEXT DEFAULT '',
                PaidDate TEXT,
                IsPaid INTEGER NOT NULL DEFAULT 0,
                FOREIGN KEY (EmployeeId) REFERENCES Employees(Id)
            );

            CREATE TABLE IF NOT EXISTS Attendances (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                EmployeeId INTEGER NOT NULL,
                Date TEXT NOT NULL,
                CheckIn TEXT DEFAULT '',
                CheckOut TEXT DEFAULT '',
                Status TEXT NOT NULL DEFAULT 'Có mặt',
                Notes TEXT DEFAULT '',
                FOREIGN KEY (EmployeeId) REFERENCES Employees(Id)
            );

            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL,
                FullName TEXT NOT NULL DEFAULT '',
                Role TEXT NOT NULL DEFAULT 'HR',
                IsActive INTEGER NOT NULL DEFAULT 1
            );
        ";
        cmd.ExecuteNonQuery();
    }

    private static void SeedDefaultData()
    {
        using var conn = GetConnection();
        conn.Open();

        // Check if admin exists
        using var checkCmd = conn.CreateCommand();
        checkCmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Username = 'admin'";
        var count = Convert.ToInt32(checkCmd.ExecuteScalar());
        if (count > 0) return;

        // Insert default admin
        using var insertUser = conn.CreateCommand();
        insertUser.CommandText = @"
            INSERT INTO Users (Username, PasswordHash, FullName, Role, IsActive)
            VALUES ('admin', @hash, 'Quản trị viên', 'Admin', 1)";
        insertUser.Parameters.AddWithValue("@hash", HashPassword("admin123"));
        insertUser.ExecuteNonQuery();

        // Insert default departments
        using var insertDept = conn.CreateCommand();
        insertDept.CommandText = @"
            INSERT INTO Departments (Name, Description) VALUES
                ('Ban Giám đốc', 'Ban lãnh đạo công ty'),
                ('Phòng Nhân sự', 'Quản lý nhân sự và tuyển dụng'),
                ('Phòng Kế toán', 'Quản lý tài chính kế toán'),
                ('Phòng Kỹ thuật', 'Phát triển và vận hành hệ thống'),
                ('Phòng Kinh doanh', 'Kinh doanh và phát triển thị trường')
        ";
        insertDept.ExecuteNonQuery();

        // Insert default positions
        using var insertPos = conn.CreateCommand();
        insertPos.CommandText = @"
            INSERT INTO Positions (Name, Description, DepartmentId) VALUES
                ('Giám đốc', 'Giám đốc điều hành', 1),
                ('Trưởng phòng Nhân sự', 'Trưởng phòng Nhân sự', 2),
                ('Chuyên viên Nhân sự', 'Chuyên viên Nhân sự', 2),
                ('Kế toán trưởng', 'Kế toán trưởng', 3),
                ('Kế toán viên', 'Kế toán viên', 3),
                ('Trưởng phòng Kỹ thuật', 'Trưởng phòng Kỹ thuật', 4),
                ('Lập trình viên', 'Lập trình viên', 4),
                ('Trưởng phòng Kinh doanh', 'Trưởng phòng Kinh doanh', 5),
                ('Nhân viên Kinh doanh', 'Nhân viên Kinh doanh', 5)
        ";
        insertPos.ExecuteNonQuery();
    }

    public static string HashPassword(string password)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes).ToLower();
    }

    public static bool VerifyPassword(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}
