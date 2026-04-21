using LabControl.Data;
using LabControl.Models;
using Microsoft.Data.Sqlite;

namespace LabControl.Services;

public class SessionService
{
    private readonly DatabaseContext _db;

    public SessionService(DatabaseContext db) => _db = db;

    public List<Session> GetAll(DateTime? from = null, DateTime? to = null)
    {
        var list = new List<Session>();
        using var cmd = _db.Connection.CreateCommand();
        var where = "1=1";
        if (from.HasValue) { where += " AND s.StartTime >= $from"; cmd.Parameters.AddWithValue("$from", from.Value.ToString("yyyy-MM-dd")); }
        if (to.HasValue) { where += " AND s.StartTime <= $to"; cmd.Parameters.AddWithValue("$to", to.Value.ToString("yyyy-MM-dd") + " 23:59:59"); }
        cmd.CommandText = $@"
            SELECT s.Id, s.StationId, s.UserId, s.StartTime, s.EndTime, s.Purpose, s.Notes,
                   st.Name, u.FirstName||' '||u.LastName, u.StudentId
            FROM Sessions s
            JOIN Stations st ON st.Id = s.StationId
            JOIN Users    u  ON u.Id  = s.UserId
            WHERE {where}
            ORDER BY s.StartTime DESC";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(MapSession(reader));
        return list;
    }

    public List<Session> GetActive()
    {
        var list = new List<Session>();
        using var cmd = _db.Connection.CreateCommand();
        cmd.CommandText = @"
            SELECT s.Id, s.StationId, s.UserId, s.StartTime, s.EndTime, s.Purpose, s.Notes,
                   st.Name, u.FirstName||' '||u.LastName, u.StudentId
            FROM Sessions s
            JOIN Stations st ON st.Id = s.StationId
            JOIN Users    u  ON u.Id  = s.UserId
            WHERE s.EndTime IS NULL
            ORDER BY s.StartTime";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(MapSession(reader));
        return list;
    }

    public int Start(Session session)
    {
        using var cmd = _db.Connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Sessions (StationId, UserId, StartTime, Purpose, Notes)
            VALUES ($stid, $uid, $start, $purpose, $notes);
            SELECT last_insert_rowid();";
        BindSession(cmd, session);
        var id = Convert.ToInt32(cmd.ExecuteScalar());

        // Mark station as in-use
        using var upd = _db.Connection.CreateCommand();
        upd.CommandText = "UPDATE Stations SET Status=1 WHERE Id=$id";
        upd.Parameters.AddWithValue("$id", session.StationId);
        upd.ExecuteNonQuery();

        return id;
    }

    public void End(int sessionId)
    {
        using (var cmd = _db.Connection.CreateCommand())
        {
            cmd.CommandText = "UPDATE Sessions SET EndTime=$end WHERE Id=$id AND EndTime IS NULL";
            cmd.Parameters.AddWithValue("$end", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("$id", sessionId);
            cmd.ExecuteNonQuery();
        }

        int stationId;
        using (var sel = _db.Connection.CreateCommand())
        {
            sel.CommandText = "SELECT StationId FROM Sessions WHERE Id=$id";
            sel.Parameters.AddWithValue("$id", sessionId);
            stationId = Convert.ToInt32(sel.ExecuteScalar());
        }

        // Mark station back to available if no other active sessions
        using var upd = _db.Connection.CreateCommand();
        upd.CommandText = @"
            UPDATE Stations SET Status=0
            WHERE Id=$stid AND NOT EXISTS (
                SELECT 1 FROM Sessions WHERE StationId=$stid AND EndTime IS NULL
            )";
        upd.Parameters.AddWithValue("$stid", stationId);
        upd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var cmd = _db.Connection.CreateCommand();
        cmd.CommandText = "DELETE FROM Sessions WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    public int GetActiveCount()
    {
        using var cmd = _db.Connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Sessions WHERE EndTime IS NULL";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public int GetTodayCount()
    {
        using var cmd = _db.Connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Sessions WHERE date(StartTime)=date('now')";
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private static Session MapSession(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0),
        StationId = r.GetInt32(1),
        UserId = r.GetInt32(2),
        StartTime = DateTime.Parse(r.GetString(3)),
        EndTime = r.IsDBNull(4) ? null : DateTime.Parse(r.GetString(4)),
        Purpose = r.GetString(5),
        Notes = r.GetString(6),
        StationName = r.GetString(7),
        UserFullName = r.GetString(8),
        UserStudentId = r.GetString(9)
    };

    private static void BindSession(SqliteCommand cmd, Session s)
    {
        cmd.Parameters.AddWithValue("$stid", s.StationId);
        cmd.Parameters.AddWithValue("$uid", s.UserId);
        cmd.Parameters.AddWithValue("$start", s.StartTime.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.Parameters.AddWithValue("$purpose", s.Purpose);
        cmd.Parameters.AddWithValue("$notes", s.Notes);
    }
}
