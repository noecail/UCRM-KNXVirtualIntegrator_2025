using System.Windows.Input;

namespace KNX_Virtual_Integrator.ViewModel.Commands;

public class RelayCommandWithMultipleArgs(Action<object[]> execute, Func<object[], bool>? canExecute = null)
    : ICommand
{
    private readonly Action<object[]> _execute = execute ?? throw new ArgumentNullException(nameof(execute));

    public bool CanExecute(object? parameter)
    {
        return parameter is object[] parameters && (canExecute?.Invoke(parameters) ?? true);
    }

    public void Execute(object? parameter)
    {
        if (parameter is object[] parameters)
        {
            _execute(parameters);
        }
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}