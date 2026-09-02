using VisitManagement.Domain.Visits;

namespace VisitManagement.Application.Abstractions;

public interface IVisitRepository
{
    Task AddAsync(Visit visit, CancellationToken cancellationToken = default);
    Task<Visit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Visit> Items, int TotalCount)> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );
    Task<Visit?> UpdateAsync(Visit visit, CancellationToken cancellationToken = default);
}
