using System.Windows.Input;
using Microsoft.Maui.Controls;
using HWAY.Payroll.UI.Views;

namespace HWAY.Payroll.UI.ViewModels;

public class EmployerViewModel
{
    public ICommand GoToAddEmployerCommand { get; }
    public ICommand GoToSearchEmployerCommand { get; }
    public ICommand GoToRemoveEmployerCommand { get; }

    public EmployerViewModel()
    {
        GoToAddEmployerCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync(nameof(EmployerInsertPage));
        });

        GoToSearchEmployerCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync(nameof(EmployerUpdatePage));
        });

        GoToRemoveEmployerCommand = new Command(async () =>
        {
            await Shell.Current.GoToAsync(nameof(EmployerRemovePage));
        });
    }
}