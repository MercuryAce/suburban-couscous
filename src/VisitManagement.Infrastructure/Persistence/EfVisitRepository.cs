using Microsoft.EntityFrameworkCore;
using VisitManagement.Application.Abstractions;
using VisitManagement.Domain.Visits;
using VisitManagement.Infrastructure.Persistence.Mapping;


namespace VisitManagement.Infrastructure.Persistence;

public sealed class EfVisitRepository(VisitManagementDbContext db) : IVisitRepository
{
    public async Task AddAsync(Visit visit, CancellationToken ct = default)
    {
        db.Visits.Add(visit.ToRecord());
        await db.SaveChangesAsync(ct);
    }

    public async Task<Visit?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var record = await db.Visits
            .AsNoTracking()
            .Include(v => v.Activities)
            .FirstOrDefaultAsync(v => v.Id == id, ct);
        return record?.ToDomain();
    }

    public async Task<(IReadOnlyList<Visit> Items, int TotalCount)> GetPageAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = db.Visits.AsNoTracking().Include(v => v.Activities);
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items.Select(v => v.ToDomain()).ToList(), total);
    }

    public async Task<Visit?> UpdateAsync(Visit visit, CancellationToken ct = default)
    {
        var record = await db.Visits
            .Include(v => v.Activities)
            .FirstOrDefaultAsync(v => v.Id == visit.Id, ct);
        if (record is null)
            return null;

        var incoming = visit.ToRecord();
        record.Status = incoming.Status;
        record.VehicleLicenceNumber = incoming.VehicleLicenceNumber;
        record.VisitorId = incoming.VisitorId;
        record.VisitorFirstName = incoming.VisitorFirstName;
        record.VisitorLastName = incoming.VisitorLastName;
        record.UpdatedAt = incoming.UpdatedAt;
        record.UpdatedBy = incoming.UpdatedBy;

        db.RemoveRange(record.Activities);
        record.Activities = incoming.Activities;

        await db.SaveChangesAsync(ct);
        return visit;
    }
}
