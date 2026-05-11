using HWAY.Payroll.Services.Paystub;
using HWAY.Payroll.UI.ViewModels;

namespace HWAY.Payroll.UI.Views;

public partial class PayrollWorkspacePage : ContentPage
{
    private readonly PayrollWorkspaceViewModel _viewModel;
    private readonly IPaystubPDFService _paystubPdfService;

    public PayrollWorkspacePage(
        PayrollWorkspaceViewModel viewModel,
        IPaystubPDFService paystubPdfService)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _paystubPdfService = paystubPdfService;
        BindingContext = _viewModel;
    }

    private async void PreviewPaystubButton_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button button || button.BindingContext is not EmployeePayrollRowViewModel row)
            return;

        row.ShowPreviewPaystub();

        var paystubView = new PaystubTemplate
        {
            BindingContext = row.PreviewPaystub,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start
        };

        var titleLabel = new Label
        {
            Text = $"Paystub Preview - {row.EmployeeName}",
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
            var fileName = BuildPaystubFileName(row.EmployeeName);
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
            Title = $"Paystub Preview - {row.EmployeeName}",
            Padding = new Thickness(20),
            Content = pageGrid
        };

        await Navigation.PushModalAsync(previewPage);
    }

    private async void PrintPaystubButton_Clicked(object sender, EventArgs e)
    {
        if (sender is not Button button || button.BindingContext is not EmployeePayrollRowViewModel row)
            return;

        row.ShowPreviewPaystub();

        var paystubView = new PaystubTemplate
        {
            BindingContext = row.PreviewPaystub,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start
        };

        var titleLabel = new Label
        {
            Text = $"Preparing Paystub PDF - {row.EmployeeName}",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center
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
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = 12
        };

        headerGrid.Add(titleLabel, 0, 0);
        headerGrid.Add(closeButton, 1, 0);

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

        var printPage = new ContentPage
        {
            Title = $"Print Paystub - {row.EmployeeName}",
            Padding = new Thickness(20),
            Content = pageGrid
        };

        await Navigation.PushModalAsync(printPage);
        await Task.Delay(300);

        var fileName = BuildPaystubFileName(row.EmployeeName);
        await _paystubPdfService.ExportFromViewAsync(paystubView, fileName);
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
