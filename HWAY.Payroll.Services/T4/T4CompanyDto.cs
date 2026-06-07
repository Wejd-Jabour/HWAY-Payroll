namespace HWAY.Payroll.Services.T4;

public class T4CompanyDto
{
    public int Year { get; set; }
    public string RpNumber { get; set; } = string.Empty;
    public string EmployerName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string CalculationSource { get; set; } = string.Empty;

    public decimal Box14Total { get; set; }
    public decimal Box24Total { get; set; }
    public decimal Box26Total { get; set; }
    public decimal Box22Total { get; set; }
    public decimal Box16Total { get; set; }
    public decimal Box16ATotal { get; set; }
    public decimal Box18Total { get; set; }

    public T4TaxYearRules? TaxYearRules { get; set; }
    public List<T4EmployeeSlipDto> Slips { get; set; } = [];
}
