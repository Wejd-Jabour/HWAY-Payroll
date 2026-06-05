namespace HWAY.Payroll.Services.EmployeeUpdate;
public class EmployeeUpdateDto
{
    public string RpNumber { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string? Sin { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }
    public string? EmailAddress { get; set; }
    public DateTime? HireDate { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public bool EiExempt { get; set; }
}
