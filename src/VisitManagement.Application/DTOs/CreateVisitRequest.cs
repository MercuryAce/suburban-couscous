using VisitManagement.Domain.Visits;

namespace VisitManagement.Application.DTOs;

public sealed record CreateVisitRequest(
    VisitStatus Status,
    string VehicleLicenceNumber,
    VisitorRequest Visitor,
    IReadOnlyList<ActivityRequest> Activities);

public sealed record VisitorRequest(
    string Id,
    string FirstName,
    string LastName
);

public sealed record ActivityRequest(
    ActivityType Type,
    string TravellerNumber,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt
);