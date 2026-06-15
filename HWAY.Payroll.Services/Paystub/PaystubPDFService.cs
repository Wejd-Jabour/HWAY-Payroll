using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace HWAY.Payroll.Services.Paystub;

public class PaystubPDFService : IPaystubPDFService
{
    public async Task<string> GenerateFromViewAsync(View paystubView, string fileName)
    {
        var imageBytes = await CapturePaystubImageAsync(paystubView);

        var safeFileName = MakeSafeFileName(fileName);
        var filePath = Path.Combine(FileSystem.AppDataDirectory, safeFileName);

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(0);

                page.Content()
                    .Image(imageBytes)
                    .FitArea();
            });
        })
        .GeneratePdf(filePath);

        return filePath;
    }

    public async Task ExportFromViewAsync(View paystubView, string fileName)
    {
        var filePath = await GenerateFromViewAsync(paystubView, fileName);

        await Launcher.OpenAsync(new OpenFileRequest
        {
            File = new ReadOnlyFile(filePath)
        });
    }

    private static async Task<byte[]> CapturePaystubImageAsync(View paystubView)
    {
        // Force the paystub to its intended printable size.
        paystubView.Measure(850, 1100);
        paystubView.Layout(new Rect(0, 0, 850, 1100));

        await Task.Delay(150);

        // Important: this captures ONLY the paystub view, not the whole app screen.
        var screenshot = await paystubView.CaptureAsync();

        if (screenshot is null)
            throw new InvalidOperationException("Unable to capture the paystub view.");

        await using var imageStream = await screenshot.OpenReadAsync(ScreenshotFormat.Png);
        using var memoryStream = new MemoryStream();
        await imageStream.CopyToAsync(memoryStream);

        return memoryStream.ToArray();
    }

    private static string MakeSafeFileName(string fileName)
    {
        foreach (var invalidChar in Path.GetInvalidFileNameChars())
            fileName = fileName.Replace(invalidChar, '_');

        return fileName;
    }
}
