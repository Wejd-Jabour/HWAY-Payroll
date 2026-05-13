using System.Windows.Input;
using Microsoft.Maui.Controls;
using HWAY.Payroll.UI.Views;

namespace HWAY.Payroll.UI.ViewModels;

public class EmployeeViewModel
{
    public ICommand GoToAddEmployeeCommand { get; }
    public ICommand GoToSearchEmployeeCommand { get; }
    public ICommand GoToRemoveEmployeeCommand { get; }

    public EmployeeViewModel()
    {
        GoToAddEmployeeCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync(nameof(EmployeeInsertPage));
        });

        GoToSearchEmployeeCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync(nameof(EmployeeSearchPage));
        });

        GoToRemoveEmployeeCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync(nameof(EmployeeRemovePage));
        });
    }
}