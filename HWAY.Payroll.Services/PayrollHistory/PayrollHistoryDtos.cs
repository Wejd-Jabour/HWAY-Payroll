using System.Globalization;

namespace HWAY.Payroll.Services.PayrollHistory;

public sealed class PayrollHistorySearchRequest
{
    public string RpNumber { get; set; } = string.Empty;
    public string? EmployerGroup { get; set; }
    public int? Year { get; set; }
    public int? Month { get; set; }
    public int? Period { get; set; }
    public string? EmployeeSearchText { get; set; }
}

public sealed class PayrollHistorySearchResultDto
{
    public IReadOnlyList<PayrollHistoryRecordDto> Records { get; set; } = [];
    public int RecordCount { get; set; }
    public int EmployeeCount { get; set; }
    public decimal Gross { get; set; }
    public decimal TotalPay { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetPay { get; set; }
}

public sealed class PayrollHistoryTermOptionDto
{
    public string Value { get; set; } = string.Empty;
    public string DisplayText => Value switch
    {
        "B" => "Semi-monthly (B)",
        "C" => "Bi-weekly A (C)",
        "D" => "Bi-weekly B (D)",
        _ => "Monthly (A)"
    };
}

public sealed class PayrollHistoryMonthOptionDto
{
    public int Number { get; set; }
    public string Name => Number is >= 1 and <= 12
        ? CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Number)
        : string.Empty;
    public string DisplayText => Number is >= 1 and <= 12
        ? $"{Number:00} - {Name}"
        : string.Empty;
}

public sealed class PayrollHistoryRecordDto
{
    public int Id { get; set; }
    public int PayrollRecordId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string RpNumber { get; set; } = string.Empty;
    public string EmployerName { get; set; } = string.Empty;
    public string EmployerGroup { get; set; } = string.Empty;
    public string EmployerGroupDisplay => new PayrollHistoryTermOptionDto { Value = EmployerGroup }.DisplayText;
    public string EmployeeAddress { get; set; } = string.Empty;
    public string EmployeeCity { get; set; } = string.Empty;
    public string EmployeeProvince { get; set; } = string.Empty;
    public string EmployeePostalCode { get; set; } = string.Empty;
    public string EmployeeEmailAddress { get; set; } = string.Empty;
    public int Period { get; set; }
    public DateTime PeriodBeginning { get; set; }
    public DateTime PeriodEnding { get; set; }
    public DateTime PayDate { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public DateTime SavedAt { get; set; }
    public string PeriodBeginningDisplay => PeriodBeginning.ToString("yyyy-MM-dd");
    public string PeriodEndingDisplay => PeriodEnding.ToString("yyyy-MM-dd");
    public string PayDateDisplay => PayDate.ToString("yyyy-MM-dd");
    public string SavedAtDisplay => SavedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
    public string MonthDisplay => Month is >= 1 and <= 12
        ? CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Month)
        : string.Empty;

    public decimal Salary { get; set; }
    public decimal Rate { get; set; }
    public decimal RegularHours { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal PublicHolidayHours { get; set; }
    public decimal StatHolidayHours { get; set; }
    public decimal Vacation { get; set; }
    public decimal VacationUsed { get; set; }
    public string VacationSetting { get; set; } = string.Empty;
    public decimal Gross { get; set; }
    public decimal PaidVacation { get; set; }
    public decimal TotalPay { get; set; }

    public decimal Tax1 { get; set; }
    public decimal Cpp1 { get; set; }
    public decimal Cpp2 { get; set; }
    public decimal Ei { get; set; }
    public decimal OtherDeduction { get; set; }
    public decimal TotalDeductions { get; set; }
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
    public decimal TotalPayYtd { get; set; }
    public decimal TotalDeductionsYtd { get; set; }
    public decimal TotalPd7 { get; set; }
}
