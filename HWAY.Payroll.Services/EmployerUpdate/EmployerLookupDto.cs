namespace HWAY.Payroll.Services.EmployerUpdate;

public class EmployerLookupDto
{
    public int Id { get; set; }

    public string RpNumber { get; set; } = string.Empty;
    public string EmployerName { get; set; } = string.Empty;

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? EmployerGroup { get; set; }

    public string DisplayText => $"{RpNumber} - {EmployerName}";
}