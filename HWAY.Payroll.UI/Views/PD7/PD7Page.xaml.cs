using HWAY.Payroll.UI.ViewModel;

namespace HWAY.Payroll.UI.Views;

public partial class PD7Page : ContentPage
{
    private readonly PD7ViewModel _viewModel;

    public PD7Page(PD7ViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }

    private void OnNonNegativeNumericTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not Entry entry)
            return;

        entry.Text = e.NewTextValue?.Replace("-", string.Empty);
    }
}
