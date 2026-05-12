using Microsoft.Maui.Controls;

namespace HWAY.Payroll.Services.Paystub;

public interface IPaystubPDFService
{
    Task ExportFromViewAsync(View paystubView, string fileName);
}