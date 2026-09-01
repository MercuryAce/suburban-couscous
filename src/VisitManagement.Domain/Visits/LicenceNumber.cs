namespace VisitManagement.Domain.Visits;

public sealed class LicenceNumber : IEquatable<LicenceNumber>
{
    public string Value { get; }

    private LicenceNumber(string value) => Value = value;

    public static LicenceNumber Parse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new ArgumentException("Vehicle licence number cannot be empty");
        
        return new LicenceNumber(raw.Trim().ToUpperInvariant());
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj) => Equals(obj as LicenceNumber);

    public bool Equals(LicenceNumber? other) => other is not null && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();
}