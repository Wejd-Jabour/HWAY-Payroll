using HWAY.Payroll.UI.ViewModel;

namespace HWAY.Payroll.UI.Views;

public partial class SettingsPage : ContentPage
{
    private readonly BiWeeklySettingsViewModel _viewModel;

    public SettingsPage(BiWeeklySettingsViewModel viewModel)
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
