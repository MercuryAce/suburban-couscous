namespace VisitManagement.Domain.Visits;

public sealed class Visit
{
    public Guid Id { get; }
    public VisitStatus Status { get; private set; }
    public LicenceNumber Licence { get; private set; }
    public Visitor Visitor { get; private set; }
    public IReadOnlyList<Activity> Activities { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public string CreatedBy { get; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public string UpdatedBy { get; private set; }

    private Visit(
        Guid id, 
        VisitStatus status, 
        LicenceNumber licence, 
        Visitor visitor, 
        IReadOnlyList<Activity> activities, 
        string createdBy, 
        DateTimeOffset createdAt, 
        DateTimeOffset updatedAt, 
        string updatedBy)
    {
        Id = id;
        Status = status;
        Licence = licence;
        Visitor = visitor;
        Activities = activities;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
        UpdatedAt = updatedAt;
        UpdatedBy = updatedBy;
    }

    public static Visit Create(
        VisitStatus status, 
        string vehicleLicenceNumber, 
        Visitor? visitor, 
        IReadOnlyList<Activity> activities, 
        string createdBy, 
        DateTimeOffset utcNow)
    {
        if (visitor is null)
            throw new ArgumentException("Visitor is required.", nameof(visitor));
        if (activities is null || activities.Count == 0)
            throw new ArgumentException("At least one activity is required.", nameof(activities));
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("Created by cannot be empty.", nameof(createdBy));

        var licence = LicenceNumber.Parse(vehicleLicenceNumber);
        return new Visit(
            Guid.NewGuid(),
            status,
            licence,
            visitor,
            activities.ToList(),
            createdBy.Trim(),
            utcNow,
            utcNow,
            createdBy.Trim());
    }

    public static Visit Reconstitute(
        Guid id, 
        VisitStatus status, 
        string vehicleLicenceNumber, 
        Visitor? visitor, 
        IReadOnlyList<Activity> activities, 
        string createdBy, 
        DateTimeOffset createdAt, 
        DateTimeOffset updatedAt, 
        string updatedBy)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty");
        if (visitor is null)
            throw new ArgumentException("Visitor is required.", nameof(visitor));
        if (activities is null || activities.Count == 0)
            throw new ArgumentException("At least one activity is required.", nameof(activities));
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("Created by cannot be empty.", nameof(createdBy));
        
        var licence = LicenceNumber.Parse(vehicleLicenceNumber);
        return new Visit(id, status, licence, visitor, activities, createdBy.Trim(), createdAt, updatedAt.ToUniversalTime(), updatedBy.Trim());
    }

    public void Update(VisitStatus status, string vehicleLicenceNumber, Visitor? visitor, IReadOnlyList<Activity> activities, string updatedBy, DateTimeOffset utcNow)
    {
        if (visitor is null)
            throw new ArgumentException("Visitor is required.", nameof(visitor));
        if (activities is null || activities.Count == 0)
            throw new ArgumentException("At least one activity is required.", nameof(activities));
        if (string.IsNullOrWhiteSpace(updatedBy))
            throw new ArgumentException("Updated by cannot be empty");
        if (utcNow < CreatedAt)
            throw new ArgumentException("Updated at must be after created at");

        Status = status;
        Licence = LicenceNumber.Parse(vehicleLicenceNumber);
        Visitor = visitor;
        Activities = activities.ToList();
        UpdatedAt = utcNow;
        UpdatedBy = updatedBy.Trim();
    }

    public override string ToString() => $"{Status} {Licence} {Visitor} {Activities} {CreatedAt} {CreatedBy} {UpdatedAt} {UpdatedBy}";

    public override bool Equals(object? obj) => Equals(obj as Visit);

    public bool Equals(Visit? other) => other is not null && Id == other.Id;

    public override int GetHashCode() => Id.GetHashCode();
}