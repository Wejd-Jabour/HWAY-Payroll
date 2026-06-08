namespace HWAY.Payroll.Services.PayrollRecord;
using HWAY.Payroll.Data.Entities;

public class PayrollRecordLookupDto
{
    public int Id { get; set; }

    public string EmployeeNumber { get; set; } = string.Empty;
    public string RpNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployerName { get; set; } = string.Empty;
    public string EmployerGroup { get; set; } = string.Empty;
    public int EmployerPaystubPeriod { get; set; } = 1;
    public int Period { get; set; }
    public string EmployeeAddress { get; set; } = string.Empty;
    public string EmployeeCity { get; set; } = string.Empty;
    public string EmployeeProvince { get; set; } = string.Empty;
    public string EmployeePostalCode { get; set; } = string.Empty;
    public string EmployeeEmailAddress { get; set; } = string.Empty;

    public decimal Salary { get; set; }
    public decimal Rate { get; set; }
    public decimal RegularHours { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal PublicHolidayHours { get; set; }
    public decimal StatHolidayHours { get; set; }
    public decimal Vacation { get; set; }
    public string VacationSetting { get; set; } = HWAY.Payroll.Domain.VacationSettings.Accumulate;
    public decimal Gross { get; set; }

    public decimal Tax1 { get; set; }
    public decimal Cpp1 { get; set; }
    public decimal Cpp2 { get; set; }
    public decimal Ei { get; set; }
    public decimal OtherDeduction { get; set; }
    public decimal NetPay { get; set; }

    public decimal GrossYtd { get; set; }
    public decimal NetYtd { get; set; }
    public decimal TaxYtd { get; set; }
    public decimal Cpp1Ytd { get; set; }
    public decimal Cpp2Ytd { get; set; }
    public decimal EiYtd { get; set; }
    public decimal OtherDeductionYtd { get; set; }
    public decimal VacationYtd { get; set; }
    public decimal VacationYtdUsed { get; set; }
    public decimal VacationYtdBalance { get; set; }
    public decimal TotalPd7 { get; set; }
    public DateTime? HireDate { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public bool EiExempt { get; set; }
}
