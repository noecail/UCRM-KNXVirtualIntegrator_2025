namespace KNX_Virtual_Integrator.ViewModel.Commands;

/// <summary>
/// Represents a command that can be executed and whose execution status can be queried.
/// </summary>
public interface ICommand
{
        /// <summary>
        /// Determines whether the command can execute with the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter to check.</param>
        /// <returns>True if the command can execute; otherwise, false.</returns>
        public bool CanExecute(object? parameter) => true;

        /// <summary>
        /// Executes the command with the specified parameter.
        /// </summary>
        /// <param name="parameter">The parameter to execute the command with.</param>
        public void Execute(object? parameter);

        /// <summary>
        /// Occurs when the ability of the command to execute changes.
        /// </summary>
        public event EventHandler CanExecuteChanged;
}