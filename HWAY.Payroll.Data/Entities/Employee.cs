namespace HWAY.Payroll.Data.Entities;

public class Employee
{
    public int Id { get; set; }

    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Sin { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }
    public string? EmailAddress { get; set; }

    public ICollection<PayrollRecord> PayrollRecords { get; set; } = new List<PayrollRecord>();
}