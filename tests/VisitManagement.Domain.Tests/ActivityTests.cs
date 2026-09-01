using VisitManagement.Domain.Visits;
using Xunit;

public class ActivityTests
{
    [Fact]
    public void Create_stores_type_traveller_and_dates()
    {
        var start = DateTimeOffset.Parse("2026-09-10T00:00:00Z");
        var end = DateTimeOffset.Parse("2026-09-20T00:00:00Z");

        var activity = Activity.Create(ActivityType.Business, "TN-001", start, end);

        Assert.Equal("TN-001", activity.TravellerNumber);
        Assert.Equal(start, activity.Start);
        Assert.Equal(end, activity.End);
        Assert.Equal(ActivityType.Business, activity.Type);
    }

    [Fact]
    public void Create_assigns_new_id_when_none_supplied()
    {
        var start = DateTimeOffset.Parse("2026-09-10T00:00:00Z");
        var end = DateTimeOffset.Parse("2026-09-20T00:00:00Z");

        var activity = Activity.Create(ActivityType.Business, "TN-001", start, end);

        Assert.NotEqual(Guid.Empty, activity.Id);
    }

    [Fact]
    public void Create_rejects_blank_traveller_number()
    {
        var start = DateTimeOffset.Parse("2026-09-10T00:00:00Z");
        var end = DateTimeOffset.Parse("2026-09-20T00:00:00Z");

        Assert.Throws<ArgumentException>(() =>
            Activity.Create(ActivityType.Business, "  ", start, end));
    }

    [Fact]
    public void Create_rejects_end_not_after_start()
    {
        var start = DateTimeOffset.Parse("2026-09-20T00:00:00Z");
        var end = DateTimeOffset.Parse("2026-09-10T00:00:00Z");

        Assert.Throws<ArgumentException>(() =>
            Activity.Create(ActivityType.Business, "TN-001", start, end));
    }

    [Fact]
    public void Create_accepts_business_and_pleasure()
    {
        var start = DateTimeOffset.Parse("2026-09-10T00:00:00Z");
        var end = DateTimeOffset.Parse("2026-09-20T00:00:00Z");

        var business = Activity.Create(ActivityType.Business, "TN-001", start, end);
        Assert.Equal(ActivityType.Business, business.Type);

        var pleasure = Activity.Create(ActivityType.Pleasure, "TN-002", start, end);
        Assert.Equal(ActivityType.Pleasure, pleasure.Type);
    }

    [Fact]
    public void Create_rejects_end_equal_to_min_value()
    {
        var start = DateTimeOffset.Parse("2026-09-10T00:00:00Z");
        var end = DateTimeOffset.MinValue;

        Assert.Throws<ArgumentException>(() =>
            Activity.Create(ActivityType.Business, "TN-001", start, end));
    }
}
