using NSubstitute;
using VisitManagement.Application.Abstractions;
using VisitManagement.Application.DTOs;
using VisitManagement.Application.Usecases;
using VisitManagement.Application.Validators;
using VisitManagement.Domain.Visits;

public class GetAllVisitsTests
{
    private readonly IVisitRepository _repo = Substitute.For<IVisitRepository>();
    private readonly GetAllVisits _sut;

    public GetAllVisitsTests()
    {
        _sut = new GetAllVisits(_repo, new GetAllVisitsRequestValidator());
    }

    [Fact]
    public async Task Execute_returns_mapped_visits_when_found()
    {
        var visit = VisitTestFactory.CreateVisit();
        _repo.GetPageAsync(1, 10, Arg.Any<CancellationToken>())
            .Returns((Items: new[] { visit } as IReadOnlyList<Visit>, TotalCount: 1));

        var response = await _sut.ExecuteAsync(new GetAllVisitsRequest(1, 10));

        Assert.Single(response.Items);
        Assert.Equal(1, response.TotalCount);
        Assert.Equal("AB12 DTF", response.Items[0].VehicleLicenceNumber);
    }
}
