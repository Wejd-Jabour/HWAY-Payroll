namespace HWAY.Payroll.Domain;

public static class VacationSettings
{
    public const string Accumulate = "Accumulate";
    public const string PaidOut = "Paid Out";

    public static readonly string[] Options =
    [
        Accumulate,
        PaidOut
    ];

    public static string Normalize(string? value)
    {
        var normalized = (value ?? string.Empty)
            .Trim()
            .Replace(" ", string.Empty)
            .Replace("_", string.Empty)
            .Replace("-", string.Empty);

        return normalized.Equals("PaidOut", StringComparison.OrdinalIgnoreCase)
            ? PaidOut
            : Accumulate;
    }

    public static bool IsPaidOut(string? value) => Normalize(value) == PaidOut;
}
