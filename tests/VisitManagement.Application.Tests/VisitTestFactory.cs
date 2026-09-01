using VisitManagement.Application.DTOs;
using VisitManagement.Domain.Visits;

internal static class VisitTestFactory
{
    public static readonly DateTimeOffset Start = DateTimeOffset.Parse("2026-09-10T00:00:00Z");
    public static readonly DateTimeOffset End = DateTimeOffset.Parse("2026-09-20T00:00:00Z");
    public static readonly DateTimeOffset UtcNow = DateTimeOffset.Parse("2026-09-01T12:00:00Z");

    public static Visit CreateVisit(
        string licence = " ab12 dtf ",
        string createdBy = "client-port-ops",
        VisitStatus status = VisitStatus.Active)
    {
        var visitor = Visitor.Create("P123456", "Jane", "Doe");
        var activity = Activity.Create(ActivityType.Business, "TN-001", Start, End);
        return Visit.Create(status, licence, visitor, [activity], createdBy, UtcNow);
    }

    public static CreateVisitRequest ValidRequest() => new(
        VisitStatus.Active,
        " ab12 xyz ",
        new VisitorRequest("P123456", "Jane", "Doe"),
        [new ActivityRequest(ActivityType.Business, "TN-001", Start, End)]);
}
