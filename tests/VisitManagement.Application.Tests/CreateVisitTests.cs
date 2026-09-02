using FluentValidation;
using NSubstitute;
using VisitManagement.Application.Abstractions;
using VisitManagement.Application.Usecases;
using VisitManagement.Application.Validators;
using VisitManagement.Domain.Visits;

public class CreateVisitTests
{
    private readonly IVisitRepository _repo = Substitute.For<IVisitRepository>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly CreateVisit _sut;

    public CreateVisitTests()
    {
        _clock.UtcNow.Returns(VisitTestFactory.UtcNow);
        _sut = new CreateVisit(_repo, _clock, new CreateVisitRequestValidator());
    }

    [Fact]
    public async Task Execute_persists_normalized_licence_and_returns_response()
    {
        Visit? saved = null;
        _repo.AddAsync(Arg.Do<Visit>(v => saved = v), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var response = await _sut.ExecuteAsync(VisitTestFactory.ValidRequest(), "client-port-ops");

        Assert.NotNull(saved);
        Assert.Equal("AB12 XYZ", saved!.Licence.Value);
        Assert.Equal("client-port-ops", saved.CreatedBy);
        Assert.Equal(VisitTestFactory.UtcNow, saved.CreatedAt);
        Assert.Equal("AB12 XYZ", response.VehicleLicenceNumber);
        Assert.Equal(VisitTestFactory.Start, response.Activities[0].StartAt);
    }

    [Fact]
    public async Task Execute_does_not_call_repository_when_activities_empty()
    {
        var request = VisitTestFactory.ValidRequest() with { Activities = [] };

        await Assert.ThrowsAsync<ValidationException>(
            () => _sut.ExecuteAsync(request, "client-port-ops"));

        await _repo.DidNotReceive().AddAsync(Arg.Any<Visit>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_does_not_call_repository_when_licence_blank()
    {
        var request = VisitTestFactory.ValidRequest() with { VehicleLicenceNumber = "   " };

        await Assert.ThrowsAsync<ValidationException>(
            () => _sut.ExecuteAsync(request, "client-port-ops"));

        await _repo.DidNotReceive().AddAsync(Arg.Any<Visit>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_does_not_call_repository_when_licence_too_long()
    {
        var request = VisitTestFactory.ValidRequest() with { VehicleLicenceNumber = new string('C', 33) };

        await Assert.ThrowsAsync<ValidationException>(
            () => _sut.ExecuteAsync(request, "client-port-ops"));

        await _repo.DidNotReceive().AddAsync(Arg.Any<Visit>(), Arg.Any<CancellationToken>());
    }
}
