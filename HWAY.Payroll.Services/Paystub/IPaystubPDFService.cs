using Microsoft.Maui.Controls;

namespace HWAY.Payroll.Services.Paystub;

public interface IPaystubPDFService
{
    Task<string> GenerateFromViewAsync(View paystubView, string fileName);
    Task ExportFromViewAsync(View paystubView, string fileName);
}
