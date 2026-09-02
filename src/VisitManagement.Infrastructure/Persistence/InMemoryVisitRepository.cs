using System.Collections.Concurrent;
using VisitManagement.Application.Abstractions;
using VisitManagement.Domain.Visits;

namespace VisitManagement.Infrastructure.Persistence;

public sealed class InMemoryVisitRepository : IVisitRepository
{
    private readonly ConcurrentDictionary<Guid, Visit> _visits = new();

    public Task AddAsync(Visit visit, CancellationToken cancellationToken = default)
    {
        _visits[visit.Id] = visit;
        return Task.CompletedTask;
    }

    public Task<Visit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _visits.TryGetValue(id, out var visit);
        return Task.FromResult(visit);
    }

    public Task<(IReadOnlyList<Visit> Items, int TotalCount)> GetPageAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var ordered = _visits.Values.OrderByDescending(v => v.CreatedAt).ToList();
        var items = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return Task.FromResult(((IReadOnlyList<Visit>)items, ordered.Count));
    }

    public Task<Visit?> UpdateAsync(Visit visit, CancellationToken cancellationToken = default)
    {
        _visits[visit.Id] = visit;
        return Task.FromResult<Visit?>(visit);
    }
}
