using System.Windows.Input;

namespace KNX_Virtual_Integrator.ViewModel;

// Classe RelayCommand générique
public class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Func<T, bool>? _canExecute;

    public RelayCommand(Action<T> execute, Func<T, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter)
    {
        // Assurez-vous que le paramètre est du type attendu
        if (parameter is T tParameter)
        {
            return _canExecute?.Invoke(tParameter) ?? true;
        }
        return false;
    }

    public void Execute(object? parameter)
    {
        // Assurez-vous que le paramètre est du type attendu
        if (parameter is T tParameter)
        {
            _execute(tParameter);
        }
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}