using HWAY.Payroll.Services.Paystub;
using System.Net.Mail;
using HWAY.Payroll.Services.Email;
using HWAY.Payroll.UI.ViewModels;

namespace HWAY.Payroll.UI.Views;

public partial class PayrollWorkspacePage : ContentPage
{
    private readonly PayrollWorkspaceViewModel _viewModel;
    private readonly IPaystubPDFService _paystubPdfService;
    private readonly IPaystubEmailService _paystubEmailService;
    private readonly IEmailSettingsService _emailSettingsService;

    public PayrollWorkspacePage(
        PayrollWorkspaceViewModel viewModel,
        IPaystubPDFService paystubPdfService,
        IPaystubEmailService paystubEmailService,
        IEmailSettingsService emailSettingsService)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _paystubPdfService = paystubPdfService;
        _paystubEmailService = paystubEmailService;
        _emailSettingsService = emailSettingsService;
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

    private async void SendAllPaystubsButton_Clicked(object sender, EventArgs e)
    {
        if (_viewModel.IsBusy)
            return;

        var rows = _viewModel.EmployeePayrollRows.ToList();

        if (rows.Count == 0)
        {
            await DisplayAlert("Pay Stubs", "There are no employee pay stubs to send.", "OK");
            return;
        }

        var settings = await _emailSettingsService.GetSettingsAsync();

        if (!settings.IsConfigured)
        {
            await DisplayAlert(
                "Email Settings",
                "The configured SMTP email settings are incomplete.",
                "OK");
            return;
        }

        var rowsWithEmail = rows
            .Where(row => IsValidEmailAddress(row.EmployeeEmailAddress))
            .ToList();
        var skippedRows = rows.Count - rowsWithEmail.Count;

        if (rowsWithEmail.Count == 0)
        {
            await DisplayAlert("Pay Stubs", "None of the employee rows have a valid email address.", "OK");
            return;
        }

        if (skippedRows > 0)
        {
            var continueSending = await DisplayAlert(
                "Missing Emails",
                $"{skippedRows} employee pay stub(s) will be skipped because the email address is missing or invalid.",
                "Send Available",
                "Cancel");

            if (!continueSending)
                return;
        }

        await SendPaystubsAsync(rowsWithEmail, skippedRows);
    }

    private async Task SendPaystubsAsync(
        IReadOnlyList<EmployeePayrollRowViewModel> rows,
        int skippedRows)
    {
        var statusLabel = new Label
        {
            Text = "Preparing pay stubs...",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#0F172A")
        };
        var paystubHost = new ContentView();
        var scrollView = new ScrollView
        {
            Orientation = ScrollOrientation.Both,
            BackgroundColor = Colors.White,
            Content = paystubHost
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

        progressGrid.Add(statusLabel, 0, 0);
        progressGrid.Add(scrollView, 0, 1);

        var progressPage = new ContentPage
        {
            Title = "Send Pay Stubs",
            Padding = new Thickness(20),
            Content = progressGrid
        };

        var sent = 0;
        var failures = new List<string>();

        try
        {
            _viewModel.IsBusy = true;
            _viewModel.Message = "Sending pay stubs...";

            await Navigation.PushModalAsync(progressPage);

            for (var index = 0; index < rows.Count; index++)
            {
                var row = rows[index];
                statusLabel.Text = $"Sending {index + 1} of {rows.Count}: {row.EmployeeName}";

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

                    var fileName = BuildPaystubFileName(row.EmployeeName);
                    var filePath = await _paystubPdfService.GenerateFromViewAsync(paystubView, fileName);

                    await _paystubEmailService.SendPaystubAsync(new PaystubEmailMessage
                    {
                        RecipientEmail = row.EmployeeEmailAddress,
                        RecipientName = row.EmployeeName,
                        Subject = BuildPaystubEmailSubject(row),
                        Body = BuildPaystubEmailBody(row),
                        AttachmentPath = filePath
                    });

                    sent++;
                }
                catch (Exception ex)
                {
                    failures.Add($"{row.EmployeeName}: {ex.Message}");
                }
            }
        }
        finally
        {
            if (Navigation.ModalStack.Contains(progressPage))
            {
                await Navigation.PopModalAsync();
            }

            _viewModel.IsBusy = false;
        }

        _viewModel.Message = BuildSendSummary(sent, skippedRows, failures.Count);

        await DisplayAlert(
            "Pay Stubs",
            BuildSendAlertMessage(sent, skippedRows, failures),
            "OK");
    }

    private static string BuildPaystubFileName(string employeeName)
    {
        var safeEmployeeName = MakeSafeFileNamePart(employeeName);
        var currentDate = DateTime.Today.ToString("yyyy-MM-dd");

        return $"{safeEmployeeName}_{currentDate}.pdf";
    }

    private static string BuildPaystubEmailSubject(EmployeePayrollRowViewModel row) =>
        $"Pay stub - {row.PayDate:yyyy-MM-dd}";

    private static string BuildPaystubEmailBody(EmployeePayrollRowViewModel row) =>
        $"Hello {row.EmployeeName},\n\nPlease find attached your pay stub for pay date {row.PayDate:yyyy-MM-dd}.\n\nThank you.";

    private static string BuildSendSummary(int sent, int skipped, int failed) =>
        $"Sent {sent} pay stub(s). Skipped {skipped}. Failed {failed}.";

    private static string BuildSendAlertMessage(int sent, int skipped, IReadOnlyList<string> failures)
    {
        var message = BuildSendSummary(sent, skipped, failures.Count);

        if (failures.Count == 0)
            return message;

        var failureDetails = string.Join("\n", failures.Take(5));
        var moreFailures = failures.Count > 5
            ? $"\n...and {failures.Count - 5} more."
            : "";

        return $"{message}\n\n{failureDetails}{moreFailures}";
    }

    private static bool IsValidEmailAddress(string emailAddress)
    {
        if (string.IsNullOrWhiteSpace(emailAddress))
            return false;

        try
        {
            var parsed = new MailAddress(emailAddress);
            return parsed.Address.Equals(emailAddress.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static string MakeSafeFileNamePart(string value)
    {
        foreach (var invalidChar in Path.GetInvalidFileNameChars())
            value = value.Replace(invalidChar, '_');

        return value.Trim();
    }
}
