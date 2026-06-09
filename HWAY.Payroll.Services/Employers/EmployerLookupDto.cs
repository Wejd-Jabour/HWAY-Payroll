namespace HWAY.Payroll.Services.Employers;

public class EmployerLookupDto
{
    public int Id { get; set; }

    public string RpNumber { get; set; } = string.Empty;
    public string EmployerName { get; set; } = string.Empty;

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? EmployerGroup { get; set; }
    public int PaystubPeriod { get; set; } = 1;
    public decimal Fee { get; set; }
    public decimal PD7Total { get; set; }
    public int PD7TotalYear { get; set; }

    public string DisplayText => $"{RpNumber} - {EmployerName}";
    public string PD7TotalDisplay => $"PD7 Total ({PD7TotalYear}): {PD7Total:0.00}";
}
