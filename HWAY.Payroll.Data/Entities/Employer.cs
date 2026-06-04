namespace HWAY.Payroll.Data.Entities;

public class Employer
{
    public int Id { get; set; }

    public string RpNumber { get; set; } = string.Empty;
    public string EmployerName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? EmployerGroup { get; set; }
    public decimal Fee { get; set; }

    public ICollection<PayrollRecord> PayrollRecords { get; set; } = new List<PayrollRecord>();
}
