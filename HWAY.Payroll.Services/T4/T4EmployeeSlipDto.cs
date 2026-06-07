namespace HWAY.Payroll.Services.T4;

public class T4EmployeeSlipDto
{
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string Sin { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;

    public decimal Box14EmploymentIncome { get; set; }
    public decimal Box24EiInsurableEarnings { get; set; }
    public decimal Box26CppQppPensionableEarnings { get; set; }
    public decimal Box22IncomeTaxDeducted { get; set; }
    public decimal Box16CppContributions { get; set; }
    public decimal Box16ACpp2Contributions { get; set; }
    public decimal Box18EiPremiums { get; set; }
}
