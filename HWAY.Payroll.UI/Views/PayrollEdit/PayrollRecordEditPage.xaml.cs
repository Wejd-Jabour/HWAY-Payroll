using HWAY.Payroll.Services.Paystub;
using HWAY.Payroll.UI.ViewModels;

namespace HWAY.Payroll.UI.Views;
public partial class PayrollRecordEditPage : ContentPage
{
    private readonly PayrollRecordEditViewModel _viewModel;
    private readonly IPaystubPDFService _paystubPdfService;

    public PayrollRecordEditPage(
        PayrollRecordEditViewModel viewModel,
        IPaystubPDFService paystubPdfService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _paystubPdfService = paystubPdfService;
        BindingContext = _viewModel;
    }

    private async void PreviewPaystubButton_Clicked(object sender, EventArgs e)
    {
        var paystub = await _viewModel.BuildPaystubPreviewAsync();

        if (paystub == null)
            return;

        await ShowPaystubAsync(paystub, "Paystub Preview", false);
    }

    private async void PrintPaystubButton_Clicked(object sender, EventArgs e)
    {
        var paystub = await _viewModel.BuildPaystubPreviewAsync();

        if (paystub == null)
            return;

        await ShowPaystubAsync(paystub, "Print Paystub", true);
    }

    private async Task ShowPaystubAsync(object paystub, string title, bool printImmediately)
    {
        var paystubView = new PaystubTemplate
        {
            BindingContext = paystub,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start
        };

        var titleLabel = new Label
        {
            Text = $"{title} - {_viewModel.EmployeeName}",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center
        };

        var printButton = new Button
        {
            Text = "Print",
            HorizontalOptions = LayoutOptions.End
        };

        printButton.Clicked += async (_, _) =>
        {
            var fileName = BuildPaystubFileName(_viewModel.EmployeeName);
            await _paystubPdfService.ExportFromViewAsync(paystubView, fileName);
        };

        var closeButton = new Button
        {
            Text = "Close",
            HorizontalOptions = LayoutOptions.End,
            Command = new Command(async () => await Navigation.PopModalAsync())
        };

        var headerGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = 12
        };

        headerGrid.Add(titleLabel, 0, 0);
        headerGrid.Add(printButton, 1, 0);
        headerGrid.Add(closeButton, 2, 0);

        var scrollView = new ScrollView
        {
            Orientation = ScrollOrientation.Both,
            BackgroundColor = Colors.White,
            Content = paystubView
        };

        var pageGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            },
            RowSpacing = 12
        };

        pageGrid.Add(headerGrid, 0, 0);
        pageGrid.Add(scrollView, 0, 1);

        var previewPage = new ContentPage
        {
            Title = $"{title} - {_viewModel.EmployeeName}",
            Padding = new Thickness(20),
            Content = pageGrid
        };

        await Navigation.PushModalAsync(previewPage);

        if (printImmediately)
        {
            await Task.Delay(300);
            var fileName = BuildPaystubFileName(_viewModel.EmployeeName);
            await _paystubPdfService.ExportFromViewAsync(paystubView, fileName);
        }
    }

    private static string BuildPaystubFileName(string employeeName)
    {
        var safeEmployeeName = MakeSafeFileNamePart(employeeName);
        var currentDate = DateTime.Today.ToString("yyyy-MM-dd");

        return $"{safeEmployeeName}_{currentDate}.pdf";
    }

    private static string MakeSafeFileNamePart(string value)
    {
        foreach (var invalidChar in Path.GetInvalidFileNameChars())
            value = value.Replace(invalidChar, '_');

        return value.Trim();
    }
}
