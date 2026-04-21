using Microsoft.Data.Sqlite;

namespace LabControl.Data;

public sealed class DatabaseContext : IDisposable
{
    private static readonly string DbPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "LabControl",
        "labcontrol.db");

    private readonly SqliteConnection _connection;

    public DatabaseContext()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(DbPath)!);
        _connection = new SqliteConnection($"Data Source={DbPath}");
        _connection.Open();
        InitializeSchema();
    }

    public SqliteConnection Connection => _connection;

    private void InitializeSchema()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            PRAGMA journal_mode=WAL;

            CREATE TABLE IF NOT EXISTS Stations (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                Name        TEXT    NOT NULL,
                IpAddress   TEXT    NOT NULL DEFAULT '',
                Location    TEXT    NOT NULL DEFAULT '',
                Status      INTEGER NOT NULL DEFAULT 0,
                OperatingSystem TEXT NOT NULL DEFAULT '',
                Notes       TEXT    NOT NULL DEFAULT '',
                DateAdded   TEXT    NOT NULL DEFAULT (datetime('now'))
            );

            CREATE TABLE IF NOT EXISTS Users (
                Id               INTEGER PRIMARY KEY AUTOINCREMENT,
                StudentId        TEXT    NOT NULL UNIQUE,
                FirstName        TEXT    NOT NULL,
                LastName         TEXT    NOT NULL,
                Department       TEXT    NOT NULL DEFAULT '',
                Email            TEXT    NOT NULL DEFAULT '',
                IsActive         INTEGER NOT NULL DEFAULT 1,
                DateRegistered   TEXT    NOT NULL DEFAULT (datetime('now'))
            );

            CREATE TABLE IF NOT EXISTS Sessions (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                StationId   INTEGER NOT NULL REFERENCES Stations(Id),
                UserId      INTEGER NOT NULL REFERENCES Users(Id),
                StartTime   TEXT    NOT NULL DEFAULT (datetime('now')),
                EndTime     TEXT,
                Purpose     TEXT    NOT NULL DEFAULT '',
                Notes       TEXT    NOT NULL DEFAULT ''
            );

            CREATE INDEX IF NOT EXISTS idx_sessions_station ON Sessions(StationId);
            CREATE INDEX IF NOT EXISTS idx_sessions_user    ON Sessions(UserId);
            CREATE INDEX IF NOT EXISTS idx_sessions_active  ON Sessions(EndTime) WHERE EndTime IS NULL;
        ";
        cmd.ExecuteNonQuery();
    }

    public void Dispose() => _connection.Dispose();
}
