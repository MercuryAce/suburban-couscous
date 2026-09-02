using FluentValidation;
using NSubstitute;
using VisitManagement.Application.Abstractions;
using VisitManagement.Application.Usecases;
using VisitManagement.Application.Validators;
using VisitManagement.Domain.Visits;

public class UpdateVisitTests
{
    private static readonly DateTimeOffset UpdatedAt = DateTimeOffset.Parse("2026-09-02T12:00:00Z");

    private readonly IVisitRepository _repo = Substitute.For<IVisitRepository>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly UpdateVisit _sut;

    public UpdateVisitTests()
    {
        _clock.UtcNow.Returns(UpdatedAt);
        _sut = new UpdateVisit(_repo, _clock, new CreateVisitRequestValidator());
    }

    [Fact]
    public async Task Execute_updates_normalized_licence_and_returns_response()
    {
        var existing = VisitTestFactory.CreateVisit();
        Visit? saved = null;
        _repo.GetByIdAsync(existing.Id, Arg.Any<CancellationToken>()).Returns(existing);
        _repo.UpdateAsync(Arg.Do<Visit>(v => saved = v), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Visit>());

        var request = VisitTestFactory.ValidRequest() with
        {
            Status = VisitStatus.Completed,
            VehicleLicenceNumber = " xy99 zzz "
        };

        var response = await _sut.ExecuteAsync(existing.Id, request, "client-port-ops");

        Assert.NotNull(saved);
        Assert.Equal("XY99 ZZZ", saved!.Licence.Value);
        Assert.Equal(VisitStatus.Completed, saved.Status);
        Assert.Equal("client-port-ops", saved.UpdatedBy);
        Assert.Equal(UpdatedAt, saved.UpdatedAt);
        Assert.Equal("client-port-ops", saved.CreatedBy);
        Assert.Equal(VisitTestFactory.UtcNow, saved.CreatedAt);
        Assert.NotNull(response);
        Assert.Equal(existing.Id, response.Id);
        Assert.Equal("XY99 ZZZ", response.VehicleLicenceNumber);
    }

    [Fact]
    public async Task Execute_returns_null_when_visit_missing()
    {
        var id = Guid.NewGuid();
        _repo.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Visit?)null);

        var result = await _sut.ExecuteAsync(id, VisitTestFactory.ValidRequest(), "client-port-ops");

        Assert.Null(result);
        await _repo.DidNotReceive().UpdateAsync(Arg.Any<Visit>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_does_not_call_repository_when_activities_empty()
    {
        var request = VisitTestFactory.ValidRequest() with { Activities = [] };

        await Assert.ThrowsAsync<ValidationException>(
            () => _sut.ExecuteAsync(Guid.NewGuid(), request, "client-port-ops"));

        await _repo.DidNotReceive().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _repo.DidNotReceive().UpdateAsync(Arg.Any<Visit>(), Arg.Any<CancellationToken>());
    }
}
