using VisitManagement.Application.DTOs;
using VisitManagement.Domain.Visits;

namespace VisitManagement.Application.Mappers;

public static class VisitMappings
{
    public static VisitResponse ToResponse(this Visit visit)
    {
        return new(
            visit.Id,
            visit.Status,
            visit.Licence.Value,
            new VisitorResponse(visit.Visitor.Id, visit.Visitor.FirstName, visit.Visitor.LastName),
            visit.Activities.Select(a => a.ToResponse()).ToList(),
            visit.CreatedAt,
            visit.CreatedBy,
            visit.UpdatedAt,
            visit.UpdatedBy);
    }

    public static ActivityResponse ToResponse(this Activity activity) => new(
        activity.Id,
        activity.Type,
        activity.TravellerNumber,
        activity.Start,
        activity.End);

    public static IReadOnlyList<Activity> ToDomain(this IReadOnlyList<ActivityRequest> activityRequests) =>
        activityRequests.Select(a => Activity.Create(a.Type, a.TravellerNumber, a.StartAt, a.EndAt)).ToList();
}
