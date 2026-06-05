namespace HWAY.Payroll.Services;

public static class EmployeePayrollEligibility
{
    public static bool CanContributeCpp(DateTime? dateOfBirth, DateTime asOfDate) =>
        !IsUnderCppAge(dateOfBirth, asOfDate);

    public static bool IsUnderCppAge(DateTime? dateOfBirth, DateTime asOfDate)
    {
        if (!dateOfBirth.HasValue)
            return false;

        return CalculateAge(dateOfBirth.Value.Date, asOfDate.Date) < 18;
    }

    public static bool CanContributeEi(bool eiExempt) => !eiExempt;

    private static int CalculateAge(DateTime dateOfBirth, DateTime asOfDate)
    {
        var age = asOfDate.Year - dateOfBirth.Year;

        if (dateOfBirth > asOfDate.AddYears(-age))
            age--;

        return age;
    }
}
