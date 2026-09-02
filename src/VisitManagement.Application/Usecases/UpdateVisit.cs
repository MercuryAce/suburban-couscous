using FluentValidation;
using VisitManagement.Application.Abstractions;
using VisitManagement.Application.DTOs;
using VisitManagement.Application.Mappers;
using VisitManagement.Application.Validators;
using VisitManagement.Domain.Visits;

namespace VisitManagement.Application.Usecases;

public sealed class UpdateVisit
{
    private readonly IVisitRepository _visitRepository;
    private readonly IClock _clock;
    private readonly IValidator<CreateVisitRequest> _validator;

    public UpdateVisit(
        IVisitRepository visitRepository,
        IClock clock,
        CreateVisitRequestValidator validator)
    {
        _visitRepository = visitRepository;
        _clock = clock;
        _validator = validator;
    }

    public async Task<VisitResponse?> ExecuteAsync(
        Guid id,
        CreateVisitRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var visit = await _visitRepository.GetByIdAsync(id, cancellationToken);
        if (visit is null)
            return null;

        var visitor = Visitor.Create(
            request.Visitor.Id,
            request.Visitor.FirstName,
            request.Visitor.LastName);

        var activities = request.Activities.ToDomain();

        visit.Update(
            request.Status,
            request.VehicleLicenceNumber,
            visitor,
            activities,
            updatedBy,
            _clock.UtcNow);

        await _visitRepository.UpdateAsync(visit, cancellationToken);
        return visit.ToResponse();
    }
}
