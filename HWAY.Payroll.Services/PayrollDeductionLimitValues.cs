namespace HWAY.Payroll.Services;

public sealed record PayrollDeductionLimitValues(
    int Year,
    decimal Cpp1Maximum,
    decimal Cpp2Maximum,
    decimal EiMaximum);
