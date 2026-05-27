namespace HWAY.Payroll.Services.PD7;

public class PD7LookupDto
{
    public string RPNumber { get; set; } = string.Empty;
    public decimal Fee { get; set; }
    public decimal Gross { get; set; }
    public decimal PD7Total { get; set; }
    public int ActiveEmployees { get; set; }
    public string Period { get; set; } = string.Empty;
    public decimal Transfer { get; set; }
    public decimal Received { get; set; }
    public bool FeeCheck { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }

    public string DisplayText => $"{RPNumber} - {Period} - {PD7Total:C}";
}
