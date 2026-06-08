namespace HWAY.Payroll.Services.Calculations;

public interface ICalculationSettingsService
{
    Task<IReadOnlyList<CalculationDto>> GetCalculationsAsync(CancellationToken cancellationToken = default);

    Task SaveCalculationAsync(CalculationDto calculation, CancellationToken cancellationToken = default);

    Task EnsureDefaultCalculationsAsync(CancellationToken cancellationToken = default);
}
