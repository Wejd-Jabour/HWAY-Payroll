namespace HWAY.Payroll.Data.Entities;

public class PayrollDeductionLimit
{
    public int Year { get; set; }

    public decimal Cpp1Maximum { get; set; }

    public decimal Cpp2Maximum { get; set; }

    public decimal EiMaximum { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
