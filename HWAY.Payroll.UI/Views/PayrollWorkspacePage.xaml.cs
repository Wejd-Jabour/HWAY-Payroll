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

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
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

    private async void PrintAllPaystubsButton_Clicked(object sender, EventArgs e)
    {
        if (_viewModel.IsBusy)
            return;

        var rows = _viewModel.EmployeePayrollRows.ToList();

        if (rows.Count == 0)
        {
            await DisplayAlert("Pay Stubs", "There are no employee pay stubs to print.", "OK");
            return;
        }

        await PrintAllPaystubsAsync(rows);
    }

    private async Task PrintAllPaystubsAsync(IReadOnlyList<EmployeePayrollRowViewModel> rows)
    {
        var statusLabel = new Label
        {
            Text = "Preparing pay stubs...",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#0F172A")
        };

        var closeButton = new Button
        {
            Text = "Close Preview",
            HorizontalOptions = LayoutOptions.End,
            BackgroundColor = Color.FromArgb("#475569"),
            TextColor = Colors.White,
            IsEnabled = false
        };

        var paystubHost = new ContentView();
        var scrollView = new ScrollView
        {
            Orientation = ScrollOrientation.Both,
            BackgroundColor = Colors.White,
            Content = paystubHost
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
        var progressGrid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            },
            RowSpacing = 12
        };

        headerGrid.Add(statusLabel, 0, 0);
        headerGrid.Add(closeButton, 1, 0);

        progressGrid.Add(headerGrid, 0, 0);
        progressGrid.Add(scrollView, 0, 1);

        var progressPage = new ContentPage
        {
            Title = "Print Pay Stubs",
            Padding = new Thickness(20),
            Content = progressGrid
        };

        closeButton.Clicked += async (_, _) =>
        {
            closeButton.IsEnabled = false;
            await Navigation.PopModalAsync();
        };

        try
        {
            _viewModel.IsBusy = true;
            _viewModel.Message = "Opening pay stubs for printing...";

            await Navigation.PushModalAsync(progressPage);

            var opened = 0;
            var failures = new List<string>();

            for (var index = 0; index < rows.Count; index++)
            {
                var row = rows[index];
                statusLabel.Text = $"Opening {index + 1} of {rows.Count}: {row.EmployeeName}";

                try
                {
                    row.ShowPreviewPaystub();

                    var paystubView = new PaystubTemplate
                    {
                        BindingContext = row.PreviewPaystub,
                        HorizontalOptions = LayoutOptions.Start,
                        VerticalOptions = LayoutOptions.Start
                    };

                    paystubHost.Content = paystubView;
                    await Task.Delay(300);

                    await _paystubPdfService.ExportFromViewAsync(
                        paystubView,
                        BuildPaystubFileName(row.EmployeeName));

                    opened++;
                }
                catch (Exception ex)
                {
                    failures.Add($"{row.EmployeeName}: {ex.Message}");
                }
            }

            _viewModel.Message = BuildPrintAllSummary(opened, failures.Count);
            statusLabel.Text = BuildPrintAllSummary(opened, failures.Count);

            if (failures.Count > 0)
            {
                await DisplayAlert(
                    "Pay Stubs",
                    BuildPrintAllAlertMessage(opened, failures),
                    "OK");
            }
        }
        catch (Exception ex)
        {
            _viewModel.Message = $"Unable to print all pay stubs: {ex.Message}";
            statusLabel.Text = $"Unable to print all pay stubs: {ex.Message}";

            await DisplayAlert(
                "Pay Stubs",
                $"Unable to print all pay stubs: {ex.Message}",
                "OK");
        }
        finally
        {
            _viewModel.IsBusy = false;
            closeButton.IsEnabled = true;
        }
    }

    private static string BuildPaystubFileName(string employeeName)
    {
        var safeEmployeeName = MakeSafeFileNamePart(employeeName);
        var currentDate = DateTime.Today.ToString("yyyy-MM-dd");

        return $"{safeEmployeeName}_{currentDate}.pdf";
    }

    private static string BuildPrintAllSummary(int opened, int failed) =>
        $"Opened {opened} pay stub PDF(s) for printing. Failed {failed}.";

    private static string BuildPrintAllAlertMessage(int opened, IReadOnlyList<string> failures)
    {
        var message = BuildPrintAllSummary(opened, failures.Count);
        var failureDetails = string.Join("\n", failures.Take(5));
        var moreFailures = failures.Count > 5
            ? $"\n...and {failures.Count - 5} more."
            : "";

        return $"{message}\n\n{failureDetails}{moreFailures}";
    }

    private static string MakeSafeFileNamePart(string value)
    {
        foreach (var invalidChar in Path.GetInvalidFileNameChars())
            value = value.Replace(invalidChar, '_');

        return value.Trim();
    }
}
