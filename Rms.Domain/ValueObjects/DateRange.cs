namespace Rms.Domain.ValueObjects;

/// <summary>Inclusive range of dates. Invalid ranges (end before start) cannot exist.</summary>
public sealed record DateRange(DateOnly Start, DateOnly End)
{
    public DateOnly End { get; } = End >= Start
        ? End
        : throw new ArgumentException("End date cannot be before the start date.");

    public int Days => End.DayNumber - Start.DayNumber + 1;

    public bool Contains(DateOnly date) => date >= Start && date <= End;

    public bool Contains(DateRange other) => other.Start >= Start && other.End <= End;

    public bool Overlaps(DateRange other) => Start <= other.End && other.Start <= End;

    public int OverlapDays(DateRange other)
    {
        var start = Start > other.Start ? Start : other.Start;
        var end = End < other.End ? End : other.End;
        return end < start ? 0 : end.DayNumber - start.DayNumber + 1;
    }

    public static Result<DateRange> Create(DateOnly start, DateOnly end)
    {
        if (end < start)
        {
            return DomainErrors.Validation("End date cannot be before the start date.");
        }

        return new DateRange(start, end);
    }
}
