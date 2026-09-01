using NSubstitute;
using VisitManagement.Application.Abstractions;
using VisitManagement.Application.Usecases;
using VisitManagement.Domain.Visits;

public class GetVisitByIdTests
{
    private readonly IVisitRepository _repo = Substitute.For<IVisitRepository>();
    private readonly GetVisitById _sut;

    public GetVisitByIdTests()
    {
        _sut = new GetVisitById(_repo);
    }

    [Fact]
    public async Task Execute_returns_mapped_visit_when_found()
    {
        var visit = VisitTestFactory.CreateVisit();
        _repo.GetByIdAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

        var response = await _sut.ExecuteAsync(visit.Id);

        Assert.NotNull(response);
        Assert.Equal(visit.Id, response!.Id);
        Assert.Equal("AB12 DTF", response.VehicleLicenceNumber);
    }

    [Fact]
    public async Task Execute_returns_null_when_missing()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Visit?)null);

        var response = await _sut.ExecuteAsync(Guid.NewGuid());

        Assert.Null(response);
    }
}
