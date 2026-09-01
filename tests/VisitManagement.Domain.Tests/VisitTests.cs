using VisitManagement.Domain.Visits;
using Xunit;

public class VisitTests
{
    private static readonly DateTimeOffset Start = DateTimeOffset.Parse("2026-09-10T00:00:00Z");
    private static readonly DateTimeOffset End = DateTimeOffset.Parse("2026-09-20T00:00:00Z");

    private static Activity SampleActivity() =>
        Activity.Create(ActivityType.Business, "TN-001", Start, End);

    private static List<Activity> OneActivity() => [SampleActivity()];

    [Fact]
    public void Create_requires_at_least_one_activity()
    {
        var action = () => Visit.Create(
            VisitStatus.Active,
            "AB12 DTF",
            Visitor.Create("P123456", "Jane", "Doe"),
            new List<Activity>(),
            "John Doe",
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Create_requires_visitor()
    {
        var action = () => Visit.Create(
            VisitStatus.Active,
            "AB12 DTF",
            null,
            OneActivity(),
            "John Doe",
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Create_normalizes_licence_via_LicenceNumber()
    {
        var utcNow = DateTimeOffset.Parse("2026-09-01T12:00:00Z");

        var visit = Visit.Create(
            VisitStatus.Active,
            " ab12 dtf ",
            Visitor.Create("P123456", "Jane", "Doe"),
            OneActivity(),
            "John Doe",
            utcNow);

        Assert.Equal("AB12 DTF", visit.Licence.Value);
    }

    [Fact]
    public void Create_sets_id_and_audit()
    {
        var utcNow = DateTimeOffset.Parse("2026-09-01T12:00:00Z");

        var visit = Visit.Create(
            VisitStatus.Active,
            "AB12 DTF",
            Visitor.Create("P123456", "Jane", "Doe"),
            OneActivity(),
            "John Doe",
            utcNow);

        Assert.NotEqual(Guid.Empty, visit.Id);
        Assert.Equal(utcNow, visit.CreatedAt);
        Assert.Equal("John Doe", visit.CreatedBy);
        Assert.Equal(utcNow, visit.UpdatedAt);
        Assert.Equal("John Doe", visit.UpdatedBy);
    }

    [Fact]
    public void Create_copies_status()
    {
        var visit = Visit.Create(
            VisitStatus.Active,
            "AB12 DTF",
            Visitor.Create("P123456", "Jane", "Doe"),
            OneActivity(),
            "John Doe",
            DateTimeOffset.Parse("2026-09-01T12:00:00Z"));

        Assert.Equal(VisitStatus.Active, visit.Status);
    }

    [Fact]
    public void Create_rejects_blank_created_by()
    {
        var action = () => Visit.Create(
            VisitStatus.Active,
            "AB12 DTF",
            Visitor.Create("P123456", "Jane", "Doe"),
            OneActivity(),
            "",
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Update_replaces_mutable_fields_and_refreshes_updated_audit()
    {
        var createdAt = DateTimeOffset.Parse("2026-09-01T12:00:00Z");
        var updatedAt = DateTimeOffset.Parse("2026-09-02T12:00:00Z");

        var visit = Visit.Create(
            VisitStatus.Active,
            "AB12 DTF",
            Visitor.Create("P123456", "Jane", "Doe"),
            OneActivity(),
            "John Doe",
            createdAt);

        visit.Update(
            VisitStatus.Completed,
            " xy99 zzz ",
            Visitor.Create("P999999", "Alan", "Smith"),
            [Activity.Create(ActivityType.Pleasure, "TN-002", Start, End)],
            "Jane Doe",
            updatedAt);

        Assert.Equal(VisitStatus.Completed, visit.Status);
        Assert.Equal("XY99 ZZZ", visit.Licence.Value);
        Assert.Equal("P999999", visit.Visitor.Id);
        Assert.Equal("Alan", visit.Visitor.FirstName);
        Assert.Equal("Smith", visit.Visitor.LastName);
        Assert.Single(visit.Activities);
        Assert.Equal(ActivityType.Pleasure, visit.Activities[0].Type);
        Assert.Equal(createdAt, visit.CreatedAt);
        Assert.Equal("John Doe", visit.CreatedBy);
        Assert.Equal(updatedAt, visit.UpdatedAt);
        Assert.Equal("Jane Doe", visit.UpdatedBy);
    }

    [Fact]
    public void Update_still_requires_at_least_one_activity()
    {
        var visit = Visit.Create(
            VisitStatus.Active,
            "AB12 DTF",
            Visitor.Create("P123456", "Jane", "Doe"),
            OneActivity(),
            "John Doe",
            DateTimeOffset.Parse("2026-09-01T12:00:00Z"));

        var action = () => visit.Update(
            VisitStatus.Active,
            "AB12 DTF",
            Visitor.Create("P123456", "Jane", "Doe"),
            new List<Activity>(),
            "Jane Doe",
            DateTimeOffset.Parse("2026-09-02T12:00:00Z"));

        Assert.Throws<ArgumentException>(action);
    }
}
