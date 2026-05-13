using System.Windows.Input;
using Microsoft.Maui.Controls;
using HWAY.Payroll.UI.Views;

namespace HWAY.Payroll.UI.ViewModels;

public class EmployeeViewModel
{
    public ICommand GoToAddEmployeeCommand { get; }
    public ICommand GoToUpdateEmployeeCommand { get; }

    public EmployeeViewModel()
    {
        GoToAddEmployeeCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync(nameof(EmployeeInsertPage));
        });

        GoToUpdateEmployeeCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync(nameof(EmployeeUpdatePage));
        });
    }
}