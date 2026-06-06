using System.Data;
using HWAY.Payroll.Data.Context;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace HWAY.Payroll.Services.PayPeriods;

public enum BiWeeklyTable
{
    A,
    B
}

public sealed record BiWeeklyPeriod(int Number, DateTime Beginning, DateTime Ending);

public sealed record BiWeeklySchedulePreview(
    DateTime LastPeriodEnding,
    DateTime FirstPeriodBeginning,
    IReadOnlyList<BiWeeklyPeriod> Periods,
    IReadOnlyList<int> ThreePeriodMonths)
{
    public decimal M1 => ThreePeriodMonths.Count > 0 ? ThreePeriodMonths[0] : 0;
    public decimal M2 => ThreePeriodMonths.Count > 1 ? ThreePeriodMonths[1] : 0;
}

public interface IBiWeeklyScheduleService
{
    BiWeeklySchedulePreview GenerateSchedule(DateTime lastPeriodEnding);
    Task<BiWeeklySchedulePreview?> GetSavedScheduleAsync(BiWeeklyTable table, CancellationToken cancellationToken = default);
    Task SaveScheduleAsync(BiWeeklyTable table, DateTime lastPeriodEnding, CancellationToken cancellationToken = default);
}

public sealed class BiWeeklyScheduleService : IBiWeeklyScheduleService
{
    private const int ScheduleRowId = 1;
    private const int PeriodCount = 26;

    private readonly AppDbContext _dbContext;

    public BiWeeklyScheduleService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public BiWeeklySchedulePreview GenerateSchedule(DateTime lastPeriodEnding)
    {
        var normalizedLastPeriodEnding = lastPeriodEnding.Date;
        var firstPeriodBeginning = normalizedLastPeriodEnding.AddDays(1);
        var periods = Enumerable
            .Range(1, PeriodCount)
            .Select(periodNumber =>
            {
                var beginning = firstPeriodBeginning.AddDays((periodNumber - 1) * 14);
                return new BiWeeklyPeriod(periodNumber, beginning, beginning.AddDays(13));
            })
            .ToList();

        var threePeriodMonths = periods
            .GroupBy(period => new { period.Ending.Year, period.Ending.Month })
            .Where(group => group.Count() == 3)
            .OrderBy(group => group.Min(period => period.Ending))
            .Select(group => group.Key.Month)
            .Take(2)
            .ToList();

        return new BiWeeklySchedulePreview(
            normalizedLastPeriodEnding,
            firstPeriodBeginning,
            periods,
            threePeriodMonths);
    }

    public async Task<BiWeeklySchedulePreview?> GetSavedScheduleAsync(
        BiWeeklyTable table,
        CancellationToken cancellationToken = default)
    {
        var tableName = GetTableName(table);
        var sql = $"""
            select {string.Join(", ", Enumerable.Range(1, PeriodCount).Select(index => $@"""P{index}"""))}
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
                return null;
            }

            var periodBeginnings = new List<DateTime>(PeriodCount);

            for (var index = 0; index < PeriodCount; index++)
            {
                if (await reader.IsDBNullAsync(index, cancellationToken))
                {
                    return null;
                }

                periodBeginnings.Add(reader.GetDateTime(index).Date);
            }

            var lastPeriodEnding = periodBeginnings[0].AddDays(-1);
            return GenerateSchedule(lastPeriodEnding);
        }
        finally
        {
            if (closeConnection)
            {
                await connection.CloseAsync();
            }
        }
    }

    public async Task SaveScheduleAsync(
        BiWeeklyTable table,
        DateTime lastPeriodEnding,
        CancellationToken cancellationToken = default)
    {
        var schedule = GenerateSchedule(lastPeriodEnding);
        var tableName = GetTableName(table);
        var periodColumns = Enumerable.Range(1, PeriodCount).Select(index => $@"""P{index}""").ToList();
        var columns = new[] { @"""ID""" }
            .Concat(periodColumns)
            .Concat(new[] { @"""M1""", @"""M2""" });

        var values = new[] { "@id" }
            .Concat(Enumerable.Range(1, PeriodCount).Select(index => $"@p{index}"))
            .Concat(new[] { "@m1", "@m2" });

        var updateAssignments = periodColumns
            .Concat(new[] { @"""M1""", @"""M2""" })
            .Select(column => $"{column} = excluded.{column}");

        var sql = $"""
            insert into public.{tableName} ({string.Join(", ", columns)})
            values ({string.Join(", ", values)})
            on conflict ("ID") do update set
                {string.Join(",\n                ", updateAssignments)}
            """;

        var parameters = new List<NpgsqlParameter>
        {
            new("id", ScheduleRowId)
        };

        foreach (var period in schedule.Periods)
        {
            parameters.Add(new NpgsqlParameter($"p{period.Number}", NpgsqlDbType.Date)
            {
                Value = period.Beginning.Date
            });
        }

        parameters.Add(new NpgsqlParameter("m1", NpgsqlDbType.Numeric) { Value = schedule.M1 });
        parameters.Add(new NpgsqlParameter("m2", NpgsqlDbType.Numeric) { Value = schedule.M2 });

        await _dbContext.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
    }

    private static string GetTableName(BiWeeklyTable table)
    {
        return table switch
        {
            BiWeeklyTable.A => @"""bi-weekly A""",
            BiWeeklyTable.B => @"""bi-weekly B""",
            _ => throw new ArgumentOutOfRangeException(nameof(table), table, "Unknown bi-weekly table.")
        };
    }
}
