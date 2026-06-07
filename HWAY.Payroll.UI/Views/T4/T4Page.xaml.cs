using HWAY.Payroll.UI.ViewModel;

namespace HWAY.Payroll.UI.Views;

public partial class T4Page : ContentPage
{
    private readonly T4ViewModel _viewModel;

    public T4Page(T4ViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    private void OnNonNegativeNumericTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is not Entry entry)
            return;

        entry.Text = e.NewTextValue?.Replace("-", string.Empty);
    }
}
