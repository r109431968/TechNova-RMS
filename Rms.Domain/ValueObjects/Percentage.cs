namespace Rms.Domain.ValueObjects;

/// <summary>Allocation percentage, always between 1 and 100.</summary>
public sealed record Percentage
{
    public Percentage(int value)
    {
        if (value is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Percentage must be between 1 and 100.");
        }

        Value = value;
    }

    public int Value { get; }

    public static Result<Percentage> Create(int value)
    {
        if (value is < 1 or > 100)
        {
            return DomainErrors.Validation("Percentage must be between 1 and 100.");
        }

        return new Percentage(value);
    }

    public override string ToString() => $"{Value}%";
}
