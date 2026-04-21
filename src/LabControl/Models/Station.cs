namespace LabControl.Models;

public enum StationStatus
{
    Available,
    InUse,
    Offline,
    Maintenance
}

public class Station
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public StationStatus Status { get; set; } = StationStatus.Available;
    public string OperatingSystem { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime DateAdded { get; set; } = DateTime.Now;

    public override string ToString() => Name;
}
