namespace HWAY.Payroll.Services;

public static class PayrollDeductionCaps
{
    public const int SupportedYear = 2026;

    public const decimal Cpp1Maximum = 4230.45m;
    public const decimal Cpp2Maximum = 416.00m;
    public const decimal EiMaximum = 1123.07m;

    public static decimal ClampCpp1(decimal requestedAmount, decimal currentYtd, int year) =>
        ClampCurrentDeduction(requestedAmount, currentYtd, GetCpp1Maximum(year));

    public static decimal ClampCpp2(decimal requestedAmount, decimal currentYtd, int year) =>
        ClampCurrentDeduction(requestedAmount, currentYtd, GetCpp2Maximum(year));

    public static decimal ClampEi(decimal requestedAmount, decimal currentYtd, int year) =>
        ClampCurrentDeduction(requestedAmount, currentYtd, GetEiMaximum(year));

    public static bool HasCpp1Room(decimal currentYtd, int year) =>
        HasRemainingRoom(currentYtd, GetCpp1Maximum(year));

    public static bool HasCpp2Room(decimal currentYtd, int year) =>
        HasRemainingRoom(currentYtd, GetCpp2Maximum(year));

    public static bool HasEiRoom(decimal currentYtd, int year) =>
        HasRemainingRoom(currentYtd, GetEiMaximum(year));

    public static decimal GetCpp1Remaining(decimal currentYtd, int year) =>
        GetRemaining(currentYtd, GetCpp1Maximum(year));

    public static decimal GetCpp2Remaining(decimal currentYtd, int year) =>
        GetRemaining(currentYtd, GetCpp2Maximum(year));

    public static decimal GetEiRemaining(decimal currentYtd, int year) =>
        GetRemaining(currentYtd, GetEiMaximum(year));

    private static decimal? GetCpp1Maximum(int year) =>
        year == SupportedYear ? Cpp1Maximum : null;

    private static decimal? GetCpp2Maximum(int year) =>
        year == SupportedYear ? Cpp2Maximum : null;

    private static decimal? GetEiMaximum(int year) =>
        year == SupportedYear ? EiMaximum : null;

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
