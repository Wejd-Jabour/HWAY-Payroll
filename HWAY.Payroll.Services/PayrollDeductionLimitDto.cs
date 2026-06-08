namespace HWAY.Payroll.Services;

public sealed class PayrollDeductionLimitDto
{
    public int Year { get; set; }

    public decimal Cpp1Maximum { get; set; }

    public decimal Cpp2Maximum { get; set; }

    public decimal EiMaximum { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public PayrollDeductionLimitValues ToValues() =>
        new(Year, Cpp1Maximum, Cpp2Maximum, EiMaximum);
}
