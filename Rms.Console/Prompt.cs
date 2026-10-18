using System.Globalization;

namespace Rms.ConsoleUI;

/// <summary>Small helpers that keep asking until the input is valid.</summary>
internal static class Prompt
{
    public static string ReadString(string label)
    {
        while (true)
        {
            Console.Write($"{label}: ");
            var text = Console.ReadLine() ?? throw new EndOfStreamException("Input closed.");
            if (!string.IsNullOrWhiteSpace(text))
            {
                return text.Trim();
            }

            Console.WriteLine("  A value is required.");
        }
    }

    public static int ReadInt(string label)
    {
        while (true)
        {
            if (int.TryParse(ReadString(label), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
            {
                return value;
            }

            Console.WriteLine("  Please enter a whole number.");
        }
    }

    public static decimal ReadDecimal(string label)
    {
        while (true)
        {
            if (decimal.TryParse(ReadString(label), NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
                && value >= 0)
            {
                return value;
            }

            Console.WriteLine("  Please enter a number that is 0 or more.");
        }
    }

    public static DateOnly ReadDate(string label)
    {
        while (true)
        {
            if (DateOnly.TryParseExact(ReadString($"{label} (yyyy-MM-dd)"), "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var value))
            {
                return value;
            }

            Console.WriteLine("  Please use the format yyyy-MM-dd, for example 2026-12-31.");
        }
    }

    public static DateRange ReadDateRange(string startLabel, string endLabel)
    {
        while (true)
        {
            var result = DateRange.Create(ReadDate(startLabel), ReadDate(endLabel));
            if (result.IsSuccess)
            {
                return result.Value;
            }

            Console.WriteLine($"  {result.Error.Message}");
        }
    }

    public static Percentage ReadPercentage(string label)
    {
        while (true)
        {
            var result = Percentage.Create(ReadInt(label));
            if (result.IsSuccess)
            {
                return result.Value;
            }

            Console.WriteLine($"  {result.Error.Message}");
        }
    }

    public static Proficiency ReadProficiency(string label)
    {
        while (true)
        {
            var value = ReadInt(label);
            if (value is >= 1 and <= 5)
            {
                return (Proficiency)value;
            }

            Console.WriteLine("  Please enter 1 (Beginner) to 5 (Expert).");
        }
    }

    public static TEnum ReadEnum<TEnum>(string label) where TEnum : struct, Enum
    {
        var values = Enum.GetValues<TEnum>();
        Console.WriteLine($"{label}:");
        for (var i = 0; i < values.Length; i++)
        {
            Console.WriteLine($"  {i + 1}. {values[i]}");
        }

        while (true)
        {
            var choice = ReadInt("Select number");
            if (choice >= 1 && choice <= values.Length)
            {
                return values[choice - 1];
            }

            Console.WriteLine("  That number is not in the list.");
        }
    }

    public static bool ReadYesNo(string label)
    {
        while (true)
        {
            var answer = ReadString($"{label} (y/n)").ToLowerInvariant();
            if (answer is "y" or "yes")
            {
                return true;
            }

            if (answer is "n" or "no")
            {
                return false;
            }
        }
    }
}
