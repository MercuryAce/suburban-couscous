namespace VisitManagement.Domain.Visits;

public sealed class Visitor 
{

    public string Id { get; }
    public string FirstName { get; }
    public string LastName { get; }

    private Visitor(string id, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Visitor ID cannot be empty");
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("Visitor first name cannot be empty");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Visitor last name cannot be empty");

        Id = id.Trim().ToUpperInvariant();
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }

    public static Visitor Create(string id, string firstName, string lastName)
    {
        return new Visitor(id, firstName, lastName);
    }

    public override string ToString() => $"{FirstName} {LastName}";

    public override bool Equals(object? obj) => Equals(obj as Visitor);
    
    public bool Equals(Visitor? other) => other is not null && Id == other.Id;

    public override int GetHashCode() => Id.GetHashCode();
}