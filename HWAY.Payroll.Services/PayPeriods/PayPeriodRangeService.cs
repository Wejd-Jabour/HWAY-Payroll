using System.Data;
using HWAY.Payroll.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace HWAY.Payroll.Services.PayPeriods;

public sealed record PayPeriodRange(int Period, DateTime Beginning, DateTime Ending);

public interface IPayPeriodRangeService
{
    Task<PayPeriodRange> GetPayPeriodRangeAsync(
        string? employerGroup,
        int period,
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
        var start = await GetPeriodStartAsync(employerGroup, normalizedPeriod, cancellationToken)
            ?? PayPeriodRules.CalculateFallbackStart(employerGroup, normalizedPeriod, DateTime.Today.Year);

        DateTime end;

        if (normalizedPeriod < maxPeriod)
        {
            var nextStart = await GetPeriodStartAsync(employerGroup, normalizedPeriod + 1, cancellationToken);
            end = nextStart?.AddDays(-1)
                ?? PayPeriodRules.CalculateEndFromStart(employerGroup, normalizedPeriod, start);
        }
        else
        {
            end = PayPeriodRules.CalculateEndFromStart(employerGroup, normalizedPeriod, start);
        }

        return new PayPeriodRange(normalizedPeriod, start.Date, end.Date);
    }

    private async Task<DateTime?> GetPeriodStartAsync(
        string? employerGroup,
        int period,
        CancellationToken cancellationToken)
    {
        var tableName = PayPeriodRules.GetPeriodTableName(employerGroup);
        var columnName = $@"""P{period}""";
        var sql = $"""
            select {columnName}
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

            var value = await command.ExecuteScalarAsync(cancellationToken);

            if (value is null || value == DBNull.Value)
            {
                return null;
            }

            return ((DateTime)value).Date;
        }
        finally
        {
            if (closeConnection)
            {
                await connection.CloseAsync();
            }
        }
    }
}
