using FluentValidation;
using VisitManagement.Application.Abstractions;
using VisitManagement.Application.DTOs;
using VisitManagement.Application.Mappers;

namespace VisitManagement.Application.Usecases;

public sealed class GetAllVisits
{
    private readonly IVisitRepository _visitRepository;
    private readonly IValidator<GetAllVisitsRequest> _validator;

    public GetAllVisits(IVisitRepository visitRepository, IValidator<GetAllVisitsRequest> validator)
    {
        _visitRepository = visitRepository;
        _validator = validator;
    }

    public async Task<PagedVisitsResponse> ExecuteAsync(
        GetAllVisitsRequest request,
        CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var (visits, totalCount) = await _visitRepository.GetPageAsync(
            request.Page, request.PageSize, cancellationToken);

        return new PagedVisitsResponse(
            visits.Select(v => v.ToResponse()).ToList(),
            request.Page,
            request.PageSize,
            totalCount);
    }
}
