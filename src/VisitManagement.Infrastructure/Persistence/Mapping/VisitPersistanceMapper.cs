using VisitManagement.Domain.Visits;
using VisitManagement.Infrastructure.Persistence.Entities;

namespace VisitManagement.Infrastructure.Persistence.Mapping;

public static class VisitPersistanceMapper
{
    public static VisitRecord ToRecord(this Visit visit) => new()
    {
        Id = visit.Id,
        Status = visit.Status,
        VehicleLicenceNumber = visit.Licence.Value,
        VisitorId = visit.Visitor.Id,
        VisitorFirstName = visit.Visitor.FirstName,
        VisitorLastName = visit.Visitor.LastName,
        CreatedAt = visit.CreatedAt,
        CreatedBy = visit.CreatedBy,
        UpdatedAt = visit.UpdatedAt,
        UpdatedBy = visit.UpdatedBy,
        Activities = visit.Activities.Select(a => new ActivityRecord
        {
            Id = a.Id,
            VisitId = visit.Id,
            Type = a.Type,
            TravellerNumber = a.TravellerNumber,
            StartAt = a.Start,
            EndAt = a.End
        }).ToList()
    };

    public static Visit ToDomain(this VisitRecord record)
    {
        var visitor = Visitor.Create(record.VisitorId, record.VisitorFirstName, record.VisitorLastName);
        var activities = record.Activities
            .Select(a => Activity.Reconstitute(a.Id, a.Type, a.TravellerNumber, a.StartAt, a.EndAt))
            .ToList();

        return Visit.Reconstitute(
            record.Id,
            record.Status,
            record.VehicleLicenceNumber,
            visitor,
            activities,
            record.CreatedBy,
            record.CreatedAt,
            record.UpdatedAt,
            record.UpdatedBy);
    }
}