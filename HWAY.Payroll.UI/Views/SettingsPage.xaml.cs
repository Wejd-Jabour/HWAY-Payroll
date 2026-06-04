using HWAY.Payroll.UI.ViewModel;

namespace HWAY.Payroll.UI.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(BiWeeklySettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
