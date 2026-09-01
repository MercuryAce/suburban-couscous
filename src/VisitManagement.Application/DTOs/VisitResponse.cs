using VisitManagement.Domain.Visits;

namespace VisitManagement.Application.DTOs;

public sealed record VisitResponse(
    Guid Id,
    VisitStatus Status,
    string VehicleLicenceNumber,
    VisitorResponse Visitor,
    IReadOnlyList<ActivityResponse> Activities,
    DateTimeOffset CreatedAt,
    string CreatedBy,
    DateTimeOffset UpdatedAt,
    string UpdatedBy
);

public sealed record VisitorResponse(
    string Id,
    string FirstName,
    string LastName
);

public sealed record ActivityResponse(
    Guid Id,
    ActivityType Type,
    string TravellerNumber,
    DateTimeOffset StartAt,
    DateTimeOffset EndAt
);

public sealed record PagedVisitsResponse(
    IReadOnlyList<VisitResponse> Items,
    int Page,
    int PageSize,
    int TotalCount
);