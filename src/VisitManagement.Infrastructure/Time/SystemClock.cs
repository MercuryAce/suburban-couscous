using VisitManagement.Application.Abstractions;

namespace VisitManagement.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
