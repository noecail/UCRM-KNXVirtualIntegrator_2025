using System.Windows.Input;

namespace KNX_Virtual_Integrator.ViewModel.Commands;

public class RelayCommandWithResult<T, TResult>(Func<T, TResult> execute, Func<T, bool>? canExecute = null) : ICommand
{
    private readonly Func<T, TResult> _execute = execute ?? throw new ArgumentNullException(nameof(execute));

    public bool CanExecute(object? parameter)
    {
        // Assurez-vous que le paramètre est du type attendu
        if (parameter is T tParameter)
        {
            return canExecute?.Invoke(tParameter) ?? true;
        }
        return false;
    }

    public TResult ExecuteWithResult(object? parameter)
    {
        // Assurez-vous que le paramètre est du type attendu
        if (parameter is T or null)
        {
            return _execute((T?)parameter!);
        }
        throw new ArgumentException($"Parameter is not of type {typeof(T).Name}");
    }

    public void Execute(object? parameter)
    {
        ExecuteWithResult(parameter);
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}