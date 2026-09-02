using VisitManagement.Domain.Visits;

namespace VisitManagement.Infrastructure.Persistence.Entities;

public sealed class ActivityRecord
{
    public Guid Id { get; set; }
    public Guid VisitId { get; set; }
    public ActivityType Type { get; set; }
    public string TravellerNumber { get; set; } = "";
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
}