using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace HWAY.Payroll.Services.Paystub;

public class PaystubPDFService : IPaystubPDFService
{
    public async Task ExportFromViewAsync(View paystubView)
    {
        // STEP 1: ensure layout is ready
        paystubView.Measure(850, 1100);
        paystubView.Layout(new Rect(0, 0, 850, 1100));

        await Task.Delay(100); // safety for rendering

        // STEP 2: capture screenshot of the current page
        var screenshot = await Screenshot.CaptureAsync();

        // STEP 3: get image stream
        var imageStream = await screenshot.OpenReadAsync(ScreenshotFormat.Png);

        // STEP 4: save PDF
        var filePath = Path.Combine(FileSystem.AppDataDirectory, $"paystub_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(0);
                page.Content().Image(imageStream).FitWidth();
            });
        })
        .GeneratePdf(filePath);

        // STEP 5: open file
        await Launcher.OpenAsync(new OpenFileRequest
        {
            File = new ReadOnlyFile(filePath)
        });
    }
}