using System.Data;
using HWAY.Payroll.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace HWAY.Payroll.Services.PayPeriods;

public sealed record PayPeriodRange(int Period, DateTime Beginning, DateTime Ending);

public sealed record PayPeriodMonthSummary(
    int Month,
    int Year,
    IReadOnlyList<PayPeriodRange> Periods)
{
    public string PeriodSummary => FormatPeriodSummary(Periods);

    private static string FormatPeriodSummary(IReadOnlyList<PayPeriodRange> periods)
    {
        if (periods.Count == 0)
        {
            return "XX";
        }

        if (periods.Count == 1)
        {
            return $"P{periods[0].Period}";
        }

        var firstPeriod = periods[0].Period;
        var lastPeriod = periods[^1].Period;
        var isContiguous = periods
            .Select((period, index) => period.Period == firstPeriod + index)
            .All(isExpectedPeriod => isExpectedPeriod);

        return isContiguous
            ? $"P{firstPeriod}-P{lastPeriod}"
            : string.Join(", ", periods.Select(period => $"P{period.Period}"));
    }
}

public interface IPayPeriodRangeService
{
    Task<PayPeriodRange> GetPayPeriodRangeAsync(
        string? employerGroup,
        int period,
        CancellationToken cancellationToken = default);

    Task<PayPeriodMonthSummary> GetPayPeriodMonthSummaryAsync(
        string? employerGroup,
        int month,
        int year,
        CancellationToken cancellationToken = default);

    Task<PayPeriodMonthSummary?> GetClosingMonthForPeriodAsync(
        string? employerGroup,
        int period,
        int year,
        CancellationToken cancellationToken = default);
}

public sealed class PayPeriodRangeService : IPayPeriodRangeService
{
    private const int ScheduleRowId = 1;

    private readonly AppDbContext _dbContext;

    public PayPeriodRangeService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PayPeriodRange> GetPayPeriodRangeAsync(
        string? employerGroup,
        int period,
        CancellationToken cancellationToken = default)
    {
        var normalizedPeriod = PayPeriodRules.NormalizePeriod(employerGroup, period);
        var maxPeriod = PayPeriodRules.GetMaxPeriod(employerGroup);
        var periodStarts = await GetPeriodStartsAsync(employerGroup, maxPeriod, cancellationToken);
        var start = periodStarts[normalizedPeriod - 1]
            ?? PayPeriodRules.CalculateFallbackStart(employerGroup, normalizedPeriod, DateTime.Today.Year);

        DateTime end;

        if (normalizedPeriod < maxPeriod)
        {
            var nextStart = periodStarts[normalizedPeriod];
            end = nextStart?.AddDays(-1)
                ?? PayPeriodRules.CalculateEndFromStart(employerGroup, normalizedPeriod, start);
        }
        else
        {
            end = PayPeriodRules.CalculateEndFromStart(employerGroup, normalizedPeriod, start);
        }

        return new PayPeriodRange(normalizedPeriod, start.Date, end.Date);
    }

    public async Task<PayPeriodMonthSummary> GetPayPeriodMonthSummaryAsync(
        string? employerGroup,
        int month,
        int year,
        CancellationToken cancellationToken = default)
    {
        if (month < 1 || month > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), month, "Month must be between 1 and 12.");
        }

        if (year < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(year), year, "Year is required.");
        }

        var ranges = await GetPayPeriodRangesForYearAsync(employerGroup, year, cancellationToken);
        var periods = ranges
            .Where(range => range.Ending.Year == year && range.Ending.Month == month)
            .OrderBy(range => range.Period)
            .ToList();

        return new PayPeriodMonthSummary(month, year, periods);
    }

    public async Task<PayPeriodMonthSummary?> GetClosingMonthForPeriodAsync(
        string? employerGroup,
        int period,
        int year,
        CancellationToken cancellationToken = default)
    {
        var normalizedPeriod = PayPeriodRules.NormalizePeriod(employerGroup, period);
        var ranges = await GetPayPeriodRangesForYearAsync(employerGroup, year, cancellationToken);
        var savedPeriodRange = ranges.FirstOrDefault(range => range.Period == normalizedPeriod);

        if (savedPeriodRange is null)
        {
            return null;
        }

        var monthPeriods = ranges
            .Where(range =>
                range.Ending.Year == savedPeriodRange.Ending.Year &&
                range.Ending.Month == savedPeriodRange.Ending.Month)
            .OrderBy(range => range.Period)
            .ToList();

        if (monthPeriods.Count == 0 || monthPeriods[^1].Period != normalizedPeriod)
        {
            return null;
        }

        return new PayPeriodMonthSummary(
            savedPeriodRange.Ending.Month,
            savedPeriodRange.Ending.Year,
            monthPeriods);
    }

    private async Task<IReadOnlyList<PayPeriodRange>> GetPayPeriodRangesForYearAsync(
        string? employerGroup,
        int year,
        CancellationToken cancellationToken)
    {
        var group = PayPeriodRules.NormalizeEmployerGroup(employerGroup);
        var maxPeriod = PayPeriodRules.GetMaxPeriod(group);
        var storedStarts = await GetPeriodStartsAsync(group, maxPeriod, cancellationToken);
        var useStoredStarts = ShouldUseStoredStarts(group, storedStarts, year);
        var starts = new List<DateTime>(maxPeriod);

        for (var period = 1; period <= maxPeriod; period++)
        {
            starts.Add((useStoredStarts
                    ? storedStarts[period - 1]
                    : null)
                ?? PayPeriodRules.CalculateFallbackStart(group, period, year));
        }

        var ranges = new List<PayPeriodRange>(maxPeriod);

        for (var index = 0; index < starts.Count; index++)
        {
            var period = index + 1;
            var start = starts[index].Date;
            var end = index < starts.Count - 1
                ? starts[index + 1].AddDays(-1).Date
                : PayPeriodRules.CalculateEndFromStart(group, period, start).Date;

            ranges.Add(new PayPeriodRange(period, start, end));
        }

        return ranges;
    }

    private async Task<IReadOnlyList<DateTime?>> GetPeriodStartsAsync(
        string? employerGroup,
        int maxPeriod,
        CancellationToken cancellationToken)
    {
        var tableName = PayPeriodRules.GetPeriodTableName(employerGroup);
        var periodColumns = string.Join(
            ", ",
            Enumerable.Range(1, maxPeriod).Select(period => $@"""P{period}"""));
        var sql = $"""
            select {periodColumns}
            from public.{tableName}
            where "ID" = @id
            """;

        var connection = _dbContext.Database.GetDbConnection();
        var closeConnection = connection.State != ConnectionState.Open;

        try
        {
            if (closeConnection)
            {
                await connection.OpenAsync(cancellationToken);
            }

            await using var command = connection.CreateCommand();
            command.CommandText = sql;

            var idParameter = command.CreateParameter();
            idParameter.ParameterName = "id";
            idParameter.Value = ScheduleRowId;
            command.Parameters.Add(idParameter);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
            {
                return Enumerable.Repeat<DateTime?>(null, maxPeriod).ToList();
            }

            var periodStarts = new List<DateTime?>(maxPeriod);

            for (var index = 0; index < maxPeriod; index++)
            {
                if (await reader.IsDBNullAsync(index, cancellationToken))
                {
                    periodStarts.Add(null);
                    continue;
                }

                periodStarts.Add(reader.GetDateTime(index).Date);
            }

            return periodStarts;
        }
        finally
        {
            if (closeConnection)
            {
                await connection.CloseAsync();
            }
        }
    }

    private static bool ShouldUseStoredStarts(
        string employerGroup,
        IReadOnlyList<DateTime?> storedStarts,
        int year)
    {
        var availableStarts = storedStarts
            .Where(start => start.HasValue)
            .Select(start => start!.Value.Date)
            .ToList();

        if (availableStarts.Count == 0)
        {
            return false;
        }

        return employerGroup switch
        {
            "C" or "D" => availableStarts.Any(start =>
                start.Year == year ||
                start.AddDays(13).Year == year),
            _ => availableStarts.Any(start => start.Year == year)
        };
    }
}
