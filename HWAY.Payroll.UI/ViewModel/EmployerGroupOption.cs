namespace HWAY.Payroll.UI.ViewModels;

public sealed record EmployerGroupOption(string Code, string DisplayName)
{
    public static EmployerGroupOption Default { get; } = new("A", "A - Monthly");

    public static IReadOnlyList<EmployerGroupOption> All { get; } =
    [
        Default,
        new("B", "B - Semi-Monthly"),
        new("C", "C - Bi-weekly A"),
        new("D", "D - Bi-weekly B")
    ];

    public static EmployerGroupOption FromCode(string? code)
    {
        var normalizedCode = NormalizeCode(code);
        return All.First(option => option.Code == normalizedCode);
    }

    public static string NormalizeCode(string? code)
    {
        var normalizedCode = code?.Trim().ToUpperInvariant();
        return All.Any(option => option.Code == normalizedCode)
            ? normalizedCode!
            : Default.Code;
    }

    public override string ToString() => DisplayName;
}
