namespace HWAY.Payroll.Services.PayPeriods;

public static class PayPeriodRules
{
    public const int MonthlyPeriodCount = 12;
    public const int SemiMonthlyPeriodCount = 24;
    public const int BiWeeklyPeriodCount = 26;

    public static string NormalizeEmployerGroup(string? employerGroup)
    {
        var group = employerGroup?.Trim().ToUpperInvariant();

        return group switch
        {
            "B" => "B",
            "C" => "C",
            "D" => "D",
            _ => "A"
        };
    }

    public static int GetMaxPeriod(string? employerGroup)
    {
        return NormalizeEmployerGroup(employerGroup) switch
        {
            "B" => SemiMonthlyPeriodCount,
            "C" or "D" => BiWeeklyPeriodCount,
            _ => MonthlyPeriodCount
        };
    }

    public static int NormalizePeriod(string? employerGroup, int period)
    {
        var maxPeriod = GetMaxPeriod(employerGroup);

        if (period < 1)
        {
            return 1;
        }

        if (period > maxPeriod)
        {
            return maxPeriod;
        }

        return period;
    }

    public static int GetNextPeriod(string? employerGroup, int period)
    {
        var normalizedPeriod = NormalizePeriod(employerGroup, period);
        var maxPeriod = GetMaxPeriod(employerGroup);

        return normalizedPeriod >= maxPeriod ? 1 : normalizedPeriod + 1;
    }

    public static string GetPeriodTableName(string? employerGroup)
    {
        return NormalizeEmployerGroup(employerGroup) switch
        {
            "B" => @"""semi-monthly""",
            "C" => @"""bi-weekly A""",
            "D" => @"""bi-weekly B""",
            _ => @"""monthly"""
        };
    }

    public static DateTime CalculateFallbackStart(string? employerGroup, int period, int year)
    {
        var group = NormalizeEmployerGroup(employerGroup);
        var normalizedPeriod = NormalizePeriod(group, period);

        return group switch
        {
            "B" => GetSemiMonthlyStart(normalizedPeriod, year),
            "C" or "D" => new DateTime(year, 1, 1).AddDays((normalizedPeriod - 1) * 14),
            _ => new DateTime(year, normalizedPeriod, 1)
        };
    }

    public static DateTime CalculateEndFromStart(string? employerGroup, int period, DateTime start)
    {
        var group = NormalizeEmployerGroup(employerGroup);
        var normalizedPeriod = NormalizePeriod(group, period);

        return group switch
        {
            "B" => normalizedPeriod % 2 == 1
                ? new DateTime(start.Year, start.Month, Math.Min(15, DateTime.DaysInMonth(start.Year, start.Month)))
                : new DateTime(start.Year, start.Month, DateTime.DaysInMonth(start.Year, start.Month)),
            "C" or "D" => start.AddDays(13),
            _ => start.AddMonths(1).AddDays(-1)
        };
    }

    private static DateTime GetSemiMonthlyStart(int period, int year)
    {
        var month = (period - 1) / 2 + 1;
        var half = (period - 1) % 2 + 1;

        return new DateTime(year, month, half == 1 ? 1 : 16);
    }
}
