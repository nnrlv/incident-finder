namespace IncidentFinder.Data.Entities;

public class Event
{
    public Guid Id { get; set; }
    public EventType Type { get; set; }
    public DateTime Time { get; set; }
}