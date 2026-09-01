namespace VisitManagement.Domain.Visits;

public sealed class Activity
{
    public Guid Id { get; }
    public ActivityType Type { get; }
    public string TravellerNumber { get; }
    public DateTimeOffset Start { get; }
    public DateTimeOffset End { get; }

    private Activity(ActivityType type, string travellerNumber, DateTimeOffset start, DateTimeOffset end)
    {
        if (string.IsNullOrWhiteSpace(travellerNumber))
            throw new ArgumentException("Traveller number cannot be empty");
        if (start >= end)
            throw new ArgumentException("Start date must be before end date");

        Id = Guid.NewGuid();
        Type = type;
        TravellerNumber = travellerNumber.Trim().ToUpperInvariant();
        Start = start.ToUniversalTime();
        End = end.ToUniversalTime();
    }

    public static Activity Create(ActivityType type, string travellerNumber, DateTimeOffset start, DateTimeOffset end)
    {
        return new Activity(type, travellerNumber, start, end);
    }

    public override string ToString() => $"{Type} {TravellerNumber} {Start} {End}";

    public override bool Equals(object? obj) => Equals(obj as Activity);

    public bool Equals(Activity? other) => other is not null && Id == other.Id;

    public override int GetHashCode() => Id.GetHashCode();
}