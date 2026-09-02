using FluentValidation;
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

    [Fact]
    public async Task Execute_passes_page_and_pageSize_to_repository()
    {
        _repo.GetPageAsync(2, 5, Arg.Any<CancellationToken>())
            .Returns((Items: Array.Empty<Visit>(), TotalCount: 12));

        var response = await _sut.ExecuteAsync(new GetAllVisitsRequest(2, 5));

        Assert.Equal(2, response.Page);
        Assert.Equal(5, response.PageSize);
        Assert.Equal(12, response.TotalCount);
        await _repo.Received(1).GetPageAsync(2, 5, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0, 50)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task Execute_does_not_call_repository_when_paging_invalid(int page, int pageSize)
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => _sut.ExecuteAsync(new GetAllVisitsRequest(page, pageSize)));

        await _repo.DidNotReceive().GetPageAsync(
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }
}
