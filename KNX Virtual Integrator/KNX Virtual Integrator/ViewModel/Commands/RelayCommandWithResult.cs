using System.Windows.Input;

namespace KNX_Virtual_Integrator.ViewModel.Commands;

/// <summary>
/// Represents a command that can be executed with a parameter and returns a result, with the ability to query whether the command can be executed.
/// </summary>
/// <typeparam name="T">The type of the parameter used by the command.</typeparam>
/// <typeparam name="TResult">The type of the result returned by the command.</typeparam>
public class RelayCommandWithResult<T, TResult> : ICommand
{
    private readonly Func<T, TResult> _execute;
    private readonly Func<T, bool>? _canExecute;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommandWithResult{T, TResult}"/> class.
    /// </summary>
    /// <param name="execute">The function to execute when the command is invoked, which returns a result.</param>
    /// <param name="canExecute">A function to determine whether the command can be executed. If null, the command can always be executed.</param>
    /// <exception cref="ArgumentNullException">Thrown when the execute function is null.</exception>
    public RelayCommandWithResult(Func<T, TResult> execute, Func<T, bool>? canExecute = null)
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
        if (parameter is T tParameter)
        {
            return _canExecute?.Invoke(tParameter) ?? true;
        }
        return false;
    }

    /// <summary>
    /// Executes the command with the specified parameter and returns the result.
    /// </summary>
    /// <param name="parameter">The parameter to execute the command with.</param>
    /// <returns>The result of executing the command.</returns>
    /// <exception cref="ArgumentException">Thrown when the parameter is not of the correct type.</exception>
    public TResult ExecuteWithResult(object? parameter)
    {
        if (parameter is T or null)
        {
            return _execute((T?)parameter!);
        }
        throw new ArgumentException($"Parameter is not of type {typeof(T).Name}");
    }

    /// <summary>
    /// Executes the command with the specified parameter. This method calls <see cref="ExecuteWithResult"/> and ignores the result.
    /// </summary>
    /// <param name="parameter">The parameter to execute the command with.</param>
    public void Execute(object? parameter)
    {
        ExecuteWithResult(parameter);
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