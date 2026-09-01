using VisitManagement.Application.Abstractions;
using VisitManagement.Application.DTOs;
using VisitManagement.Application.Mappers;

namespace VisitManagement.Application.Usecases;

public sealed class GetVisitById
{
    private readonly IVisitRepository _visitRepository;

    public GetVisitById(IVisitRepository visitRepository)
    {
        _visitRepository = visitRepository;
    }

    public async Task<VisitResponse?> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var visit = await _visitRepository.GetByIdAsync(id, cancellationToken);
        return visit?.ToResponse();
    }
}
