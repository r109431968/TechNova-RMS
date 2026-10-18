namespace Rms.Domain.ValueObjects;

/// <summary>A syntactically valid, normalised (trimmed, lower-case) email address.</summary>
public sealed record Email
{
    public Email(string value)
    {
        if (!IsValid(value))
        {
            throw new ArgumentException($"'{value}' is not a valid email address.", nameof(value));
        }

        Value = value.Trim().ToLowerInvariant();
    }

    public string Value { get; }

    public static Result<Email> Create(string value)
    {
        if (!IsValid(value))
        {
            return DomainErrors.Validation($"'{value}' is not a valid email address.");
        }

        return new Email(value);
    }

    private static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var text = value.Trim();
        var at = text.IndexOf('@');
        return at > 0
            && at == text.LastIndexOf('@')
            && text.LastIndexOf('.') > at + 1
            && !text.EndsWith('.')
            && !text.Contains(' ');
    }

    public override string ToString() => Value;
}
