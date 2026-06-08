namespace HWAY.Payroll.Services;

public static class PayrollDeductionCaps
{
    public const int SupportedYear = 2026;

    public const decimal Cpp1Maximum = 4230.45m;
    public const decimal Cpp2Maximum = 416.00m;
    public const decimal EiMaximum = 1123.07m;

    public static decimal ClampCpp1(decimal requestedAmount, decimal currentYtd, int year) =>
        ClampCpp1(requestedAmount, currentYtd, GetDefaultLimits(year));

    public static decimal ClampCpp2(decimal requestedAmount, decimal currentYtd, int year) =>
        ClampCpp2(requestedAmount, currentYtd, GetDefaultLimits(year));

    public static decimal ClampEi(decimal requestedAmount, decimal currentYtd, int year) =>
        ClampEi(requestedAmount, currentYtd, GetDefaultLimits(year));

    public static decimal ClampCpp1(
        decimal requestedAmount,
        decimal currentYtd,
        PayrollDeductionLimitValues? limits) =>
        ClampCurrentDeduction(requestedAmount, currentYtd, limits?.Cpp1Maximum);

    public static decimal ClampCpp2(
        decimal requestedAmount,
        decimal currentYtd,
        PayrollDeductionLimitValues? limits) =>
        ClampCurrentDeduction(requestedAmount, currentYtd, limits?.Cpp2Maximum);

    public static decimal ClampEi(
        decimal requestedAmount,
        decimal currentYtd,
        PayrollDeductionLimitValues? limits) =>
        ClampCurrentDeduction(requestedAmount, currentYtd, limits?.EiMaximum);

    public static bool HasCpp1Room(decimal currentYtd, int year) =>
        HasCpp1Room(currentYtd, GetDefaultLimits(year));

    public static bool HasCpp2Room(decimal currentYtd, int year) =>
        HasCpp2Room(currentYtd, GetDefaultLimits(year));

    public static bool HasEiRoom(decimal currentYtd, int year) =>
        HasEiRoom(currentYtd, GetDefaultLimits(year));

    public static bool HasCpp1Room(decimal currentYtd, PayrollDeductionLimitValues? limits) =>
        HasRemainingRoom(currentYtd, limits?.Cpp1Maximum);

    public static bool HasCpp2Room(decimal currentYtd, PayrollDeductionLimitValues? limits) =>
        HasRemainingRoom(currentYtd, limits?.Cpp2Maximum);

    public static bool HasEiRoom(decimal currentYtd, PayrollDeductionLimitValues? limits) =>
        HasRemainingRoom(currentYtd, limits?.EiMaximum);

    public static decimal GetCpp1Remaining(decimal currentYtd, int year) =>
        GetRemaining(currentYtd, GetDefaultLimits(year)?.Cpp1Maximum);

    public static decimal GetCpp2Remaining(decimal currentYtd, int year) =>
        GetRemaining(currentYtd, GetDefaultLimits(year)?.Cpp2Maximum);

    public static decimal GetEiRemaining(decimal currentYtd, int year) =>
        GetRemaining(currentYtd, GetDefaultLimits(year)?.EiMaximum);

    public static PayrollDeductionLimitValues? GetDefaultLimits(int year) =>
        year == SupportedYear
            ? new PayrollDeductionLimitValues(year, Cpp1Maximum, Cpp2Maximum, EiMaximum)
            : null;

    private static decimal ClampCurrentDeduction(
        decimal requestedAmount,
        decimal currentYtd,
        decimal? maximum)
    {
        if (maximum is null)
            return requestedAmount;

        var remaining = GetRemaining(currentYtd, maximum);

        if (requestedAmount <= 0)
            return 0;

        return Math.Min(requestedAmount, remaining);
    }

    private static bool HasRemainingRoom(decimal currentYtd, decimal? maximum) =>
        maximum is null || GetRemaining(currentYtd, maximum) > 0;

    private static decimal GetRemaining(decimal currentYtd, decimal? maximum) =>
        maximum is null ? decimal.MaxValue : Math.Max(0, maximum.Value - currentYtd);
}
