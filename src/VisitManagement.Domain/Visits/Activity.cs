namespace VisitManagement.Domain.Visits;

public sealed class Activity
{
    public Guid Id { get; }
    public ActivityType Type { get; private set; }
    public string TravellerNumber { get; private set; }
    public DateTimeOffset Start { get; private set; }
    public DateTimeOffset End { get; private set; }

    private Activity(Guid id, ActivityType type, string travellerNumber, DateTimeOffset start, DateTimeOffset end)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty");
        if (string.IsNullOrWhiteSpace(travellerNumber))
            throw new ArgumentException("Traveller number cannot be empty");
        if (start >= end)
            throw new ArgumentException("Start date must be before end date");

        Id = id;
        Type = type;
        TravellerNumber = travellerNumber.Trim().ToUpperInvariant();
        Start = start.ToUniversalTime();
        End = end.ToUniversalTime();
    }

    public static Activity Reconstitute(Guid id, ActivityType type, string travellerNumber, DateTimeOffset start, DateTimeOffset end) => new Activity(id, type, travellerNumber, start, end);

    public static Activity Create(ActivityType type, string travellerNumber, DateTimeOffset start, DateTimeOffset end) => new Activity(Guid.NewGuid(), type, travellerNumber, start, end);

    public override string ToString() => $"{Type} {TravellerNumber} {Start} {End}";

    public override bool Equals(object? obj) => Equals(obj as Activity);

    public bool Equals(Activity? other) => other is not null && Id == other.Id;

    public override int GetHashCode() => Id.GetHashCode();
}