using System;

namespace QASprintHub.Models;

public class DayNote
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime LastModified { get; set; }
}
