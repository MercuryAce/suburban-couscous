using FluentValidation;
using VisitManagement.Application.DTOs;
using VisitManagement.Application.Validators;
using VisitManagement.Application.Mappers;
using VisitManagement.Application.Abstractions;
using VisitManagement.Domain.Visits;


namespace VisitManagement.Application.Usecases;

public sealed class CreateVisit
{

    private readonly IVisitRepository _visitRepository;
    private readonly IClock _clock;
    private readonly IValidator<CreateVisitRequest> _validator;


    public CreateVisit(
        IVisitRepository visitRepository, 
        IClock clock, 
        CreateVisitRequestValidator validator)
    {
        _visitRepository = visitRepository;
        _clock = clock;
        _validator = validator;
    }

    public async Task<VisitResponse> ExecuteAsync(
        CreateVisitRequest request,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var visitor = Visitor.Create(
            request.Visitor.Id, 
            request.Visitor.FirstName, 
            request.Visitor.LastName);

        var activities = request.Activities.ToDomain();

        var visit = Visit.Create(
            request.Status,
            request.VehicleLicenceNumber,
            visitor,
            activities,
            createdBy,
            _clock.UtcNow);

        await _visitRepository.AddAsync(visit, cancellationToken);
        return visit.ToResponse();
    }
}