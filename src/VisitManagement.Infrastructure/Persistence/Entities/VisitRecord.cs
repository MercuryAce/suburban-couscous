using VisitManagement.Domain.Visits;

namespace VisitManagement.Infrastructure.Persistence.Entities;

public sealed class VisitRecord
{
    public Guid Id { get; set; }
    public VisitStatus Status { get; set; }
    public required string VehicleLicenceNumber { get; set; }
    public string VisitorId { get; set; } = "";
    public string VisitorFirstName { get; set; } = "";
    public string VisitorLastName { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public string CreatedBy { get; set; } = "";
    public DateTimeOffset UpdatedAt { get; set; }
    public string UpdatedBy { get; set; } = "";
    public List<ActivityRecord> Activities { get; set; } = [];
}
