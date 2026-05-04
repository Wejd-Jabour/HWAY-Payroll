using HWAY.Payroll.UI.ViewModels;

namespace HWAY.Payroll.UI.Views;
public partial class PayrollRecordPage : ContentPage
{
    public PayrollRecordPage(PayrollRecordViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}