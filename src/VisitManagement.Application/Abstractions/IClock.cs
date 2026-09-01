namespace VisitManagement.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}