namespace LabControl.Models;

public class LabUser
{
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime DateRegistered { get; set; } = DateTime.Now;

    public string FullName => $"{FirstName} {LastName}";

    public override string ToString() => FullName;
}
