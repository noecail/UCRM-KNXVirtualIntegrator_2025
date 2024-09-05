using System.Windows.Input;

namespace KNX_Virtual_Integrator.ViewModel.Commands;

/// <summary>
/// Represents a command that can be executed with a parameter and whose execution status can be queried.
/// </summary>
/// <typeparam name="T">The type of the parameter used by the command.</typeparam>
public class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Func<T, bool>? _canExecute;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommand{T}"/> class.
    /// </summary>
    /// <param name="execute">The action to execute when the command is invoked.</param>
    /// <param name="canExecute">A function to determine whether the command can be executed. If null, the command can always be executed.</param>
    /// <exception cref="ArgumentNullException">Thrown when the execute action is null.</exception>
    public RelayCommand(Action<T?> execute, Func<T, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>
    /// Determines whether the command can execute with the specified parameter.
    /// </summary>
    /// <param name="parameter">The parameter to check.</param>
    /// <returns>True if the command can execute; otherwise, false.</returns>
    public bool CanExecute(object? parameter)
    {
        if (parameter is T or null)
        {
            return _canExecute?.Invoke((T?)parameter!) ?? true;
        }
        return false;
    }

    /// <summary>
    /// Executes the command with the specified parameter.
    /// </summary>
    /// <param name="parameter">The parameter to execute the command with.</param>
    public void Execute(object? parameter)
    {
        if (parameter is T or null)
        {
            _execute((T?)parameter);
        }
    }

    /// <summary>
    /// Occurs when the ability of the command to execute changes.
    /// </summary>
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}