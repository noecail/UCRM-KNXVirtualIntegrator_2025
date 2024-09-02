using System.Windows.Input;

namespace KNX_Virtual_Integrator.ViewModel.Commands;

public class RelayCommand<T>(Action<T?> execute, Func<T, bool>? canExecute = null) : ICommand
{
    private readonly Action<T?> _execute = execute ?? throw new ArgumentNullException(nameof(execute));

    public bool CanExecute(object? parameter)
    {
        // Assurez-vous que le paramètre est du type attendu
        if (parameter is T or null)
        {
            return canExecute?.Invoke((T?)parameter!) ?? true;
        }
        return false;
    }

    public void Execute(object? parameter)
    {
        // Assurez-vous que le paramètre est du type attendu
        if (parameter is T or null)
        {
            _execute((T?)parameter);
        }
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}