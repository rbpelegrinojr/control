using LabControl.Data;
using LabControl.Models;
using Microsoft.Data.Sqlite;

namespace LabControl.Services;

public class UserService
{
    private readonly DatabaseContext _db;

    public UserService(DatabaseContext db) => _db = db;

    public List<LabUser> GetAll(bool includeInactive = false)
    {
        var list = new List<LabUser>();
        using var cmd = _db.Connection.CreateCommand();
        cmd.CommandText = includeInactive
            ? "SELECT Id, StudentId, FirstName, LastName, Department, Email, IsActive, DateRegistered FROM Users ORDER BY LastName, FirstName"
            : "SELECT Id, StudentId, FirstName, LastName, Department, Email, IsActive, DateRegistered FROM Users WHERE IsActive=1 ORDER BY LastName, FirstName";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(MapUser(reader));
        return list;
    }

    public LabUser? GetById(int id)
    {
        using var cmd = _db.Connection.CreateCommand();
        cmd.CommandText = "SELECT Id, StudentId, FirstName, LastName, Department, Email, IsActive, DateRegistered FROM Users WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapUser(reader) : null;
    }

    public int Add(LabUser user)
    {
        using var cmd = _db.Connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Users (StudentId, FirstName, LastName, Department, Email, IsActive)
            VALUES ($sid, $fn, $ln, $dept, $email, $active);
            SELECT last_insert_rowid();";
        BindUser(cmd, user);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void Update(LabUser user)
    {
        using var cmd = _db.Connection.CreateCommand();
        cmd.CommandText = @"
            UPDATE Users SET StudentId=$sid, FirstName=$fn, LastName=$ln,
                Department=$dept, Email=$email, IsActive=$active
            WHERE Id=$id";
        BindUser(cmd, user);
        cmd.Parameters.AddWithValue("$id", user.Id);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var cmd = _db.Connection.CreateCommand();
        cmd.CommandText = "DELETE FROM Users WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    public int GetTotalCount() => ExecuteScalarInt("SELECT COUNT(*) FROM Users WHERE IsActive=1");
    public int GetTotalCount(bool allUsers) => allUsers
        ? ExecuteScalarInt("SELECT COUNT(*) FROM Users")
        : GetTotalCount();

    private int ExecuteScalarInt(string sql)
    {
        using var cmd = _db.Connection.CreateCommand();
        cmd.CommandText = sql;
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private static LabUser MapUser(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0),
        StudentId = r.GetString(1),
        FirstName = r.GetString(2),
        LastName = r.GetString(3),
        Department = r.GetString(4),
        Email = r.GetString(5),
        IsActive = r.GetInt32(6) == 1,
        DateRegistered = DateTime.Parse(r.GetString(7))
    };

    private static void BindUser(SqliteCommand cmd, LabUser u)
    {
        cmd.Parameters.AddWithValue("$sid", u.StudentId);
        cmd.Parameters.AddWithValue("$fn", u.FirstName);
        cmd.Parameters.AddWithValue("$ln", u.LastName);
        cmd.Parameters.AddWithValue("$dept", u.Department);
        cmd.Parameters.AddWithValue("$email", u.Email);
        cmd.Parameters.AddWithValue("$active", u.IsActive ? 1 : 0);
    }
}
