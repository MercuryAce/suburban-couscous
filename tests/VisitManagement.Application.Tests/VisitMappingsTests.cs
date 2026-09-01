using VisitManagement.Application.Mappers;
using VisitManagement.Domain.Visits;
using Xunit;
using VisitManagement.Application.DTOs;

public class VisitMappingsTests
{
    [Fact]
    public void ToDomain_maps_startAt_endAt_to_domain()
    {
        var requests = new List<ActivityRequest>
        {
            new(ActivityType.Pleasure, "TN-002", VisitTestFactory.Start, VisitTestFactory.End)
        };

        var activities = requests.ToDomain();

        Assert.Equal(VisitTestFactory.Start, activities[0].Start);
        Assert.Equal(VisitTestFactory.End, activities[0].End);
    }

    [Fact]
    public void ToResponse_maps_licence_and_activity_dates()
    {
        var visit = VisitTestFactory.CreateVisit();
        var response = visit.ToResponse();

        Assert.Equal("AB12 DTF", response.VehicleLicenceNumber);
        Assert.Equal(visit.Activities[0].Start, response.Activities[0].StartAt);
        Assert.Equal(visit.Activities[0].End, response.Activities[0].EndAt);
        Assert.Equal("Jane", response.Visitor.FirstName);
        Assert.Equal("client-port-ops", response.CreatedBy);
    }
}