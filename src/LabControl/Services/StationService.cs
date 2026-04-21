using LabControl.Data;
using LabControl.Models;
using Microsoft.Data.Sqlite;

namespace LabControl.Services;

public class StationService
{
    private readonly DatabaseContext _db;

    public StationService(DatabaseContext db) => _db = db;

    public List<Station> GetAll()
    {
        var list = new List<Station>();
        using var cmd = _db.Connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, IpAddress, Location, Status, OperatingSystem, Notes, DateAdded FROM Stations ORDER BY Name";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            list.Add(MapStation(reader));
        return list;
    }

    public Station? GetById(int id)
    {
        using var cmd = _db.Connection.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, IpAddress, Location, Status, OperatingSystem, Notes, DateAdded FROM Stations WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapStation(reader) : null;
    }

    public int Add(Station station)
    {
        using var cmd = _db.Connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Stations (Name, IpAddress, Location, Status, OperatingSystem, Notes)
            VALUES ($name, $ip, $loc, $status, $os, $notes);
            SELECT last_insert_rowid();";
        BindStation(cmd, station);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void Update(Station station)
    {
        using var cmd = _db.Connection.CreateCommand();
        cmd.CommandText = @"
            UPDATE Stations SET Name=$name, IpAddress=$ip, Location=$loc,
                Status=$status, OperatingSystem=$os, Notes=$notes
            WHERE Id=$id";
        BindStation(cmd, station);
        cmd.Parameters.AddWithValue("$id", station.Id);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var cmd = _db.Connection.CreateCommand();
        cmd.CommandText = "DELETE FROM Stations WHERE Id=$id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    public Dictionary<StationStatus, int> GetStatusCounts()
    {
        var counts = new Dictionary<StationStatus, int>();
        using var cmd = _db.Connection.CreateCommand();
        cmd.CommandText = "SELECT Status, COUNT(*) FROM Stations GROUP BY Status";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            counts[(StationStatus)reader.GetInt32(0)] = reader.GetInt32(1);
        return counts;
    }

    private static Station MapStation(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0),
        Name = r.GetString(1),
        IpAddress = r.GetString(2),
        Location = r.GetString(3),
        Status = (StationStatus)r.GetInt32(4),
        OperatingSystem = r.GetString(5),
        Notes = r.GetString(6),
        DateAdded = DateTime.Parse(r.GetString(7))
    };

    private static void BindStation(SqliteCommand cmd, Station s)
    {
        cmd.Parameters.AddWithValue("$name", s.Name);
        cmd.Parameters.AddWithValue("$ip", s.IpAddress);
        cmd.Parameters.AddWithValue("$loc", s.Location);
        cmd.Parameters.AddWithValue("$status", (int)s.Status);
        cmd.Parameters.AddWithValue("$os", s.OperatingSystem);
        cmd.Parameters.AddWithValue("$notes", s.Notes);
    }
}
