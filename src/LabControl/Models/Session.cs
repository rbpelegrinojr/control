namespace LabControl.Models;

public class Session
{
    public int Id { get; set; }
    public int StationId { get; set; }
    public int UserId { get; set; }
    public DateTime StartTime { get; set; } = DateTime.Now;
    public DateTime? EndTime { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    // Navigation helpers (populated by service queries)
    public string StationName { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public string UserStudentId { get; set; } = string.Empty;

    public bool IsActive => EndTime == null;

    public string Duration
    {
        get
        {
            var end = EndTime ?? DateTime.Now;
            var span = end - StartTime;
            return span.TotalHours >= 1
                ? $"{(int)span.TotalHours}h {span.Minutes}m"
                : $"{span.Minutes}m";
        }
    }
}
