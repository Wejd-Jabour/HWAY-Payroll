using HWAY.Payroll.UI.ViewModel;

namespace HWAY.Payroll.UI.Views;

public partial class PayrollHistoryPage : ContentPage
{
    private readonly PayrollHistoryViewModel _viewModel;

    public PayrollHistoryPage(PayrollHistoryViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
