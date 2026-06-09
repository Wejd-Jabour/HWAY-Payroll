namespace HWAY.Payroll.Services.PD7;

public class PD7CalculatedTotalsDto
{
    public decimal Fee { get; set; }
    public decimal Gross { get; set; }
    public decimal NetTotal { get; set; }
    public decimal EmployerContribution { get; set; }
    public decimal PD7Total { get; set; }
    public int ActiveEmployees { get; set; }
    public string Period { get; set; } = "XX";
    public decimal Transfer { get; set; }
}
