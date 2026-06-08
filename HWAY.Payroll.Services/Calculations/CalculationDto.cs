namespace HWAY.Payroll.Services.Calculations;

public sealed class CalculationDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Calculation { get; set; } = string.Empty;
}
