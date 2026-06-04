namespace HWAY.Payroll.Services.PayrollRecord;

public class PayrollRecordUpdateDto
{
    public decimal Salary { get; set; }
    public decimal Rate { get; set; }
    public decimal RegularHours { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal PublicHolidayHours { get; set; }
    public decimal StatHolidayHours { get; set; }
    public decimal Gross { get; set; }
    public decimal Vacation { get; set; }
    public string VacationSetting { get; set; } = HWAY.Payroll.Domain.VacationSettings.Accumulate;
    public decimal VacationUsed { get; set; }
    public decimal Tax1 { get; set; }
    public decimal Cpp1 { get; set; }
    public decimal Cpp2 { get; set; }
    public decimal Ei { get; set; }
    public decimal OtherDeduction { get; set; }
    public DateTime? HireDate { get; set; }
}
