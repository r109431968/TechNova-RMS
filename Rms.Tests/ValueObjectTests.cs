namespace Rms.Tests;

public class ValueObjectTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    [InlineData(-5)]
    public void Percentage_outside_1_to_100_is_rejected(int value)
    {
        Assert.True(Percentage.Create(value).IsFailure);
        Assert.Throws<ArgumentOutOfRangeException>(() => new Percentage(value));
    }

    [Fact]
    public void DateRange_cannot_end_before_it_starts()
    {
        var result = DateRange.Create(new DateOnly(2026, 5, 1), new DateOnly(2026, 4, 1));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void DateRange_overlap_includes_shared_boundary_day()
    {
        var first = new DateRange(new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31));
        var second = new DateRange(new DateOnly(2026, 1, 31), new DateOnly(2026, 2, 28));

        Assert.True(first.Overlaps(second));
        Assert.Equal(1, first.OverlapDays(second));
    }

    [Theory]
    [InlineData("a@b.com", true)]
    [InlineData("not-an-email", false)]
    [InlineData("a@b", false)]
    public void Email_validation(string value, bool valid)
    {
        Assert.Equal(valid, Email.Create(value).IsSuccess);
    }
}
