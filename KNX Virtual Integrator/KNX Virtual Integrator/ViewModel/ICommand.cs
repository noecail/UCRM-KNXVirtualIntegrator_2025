namespace KNX_Virtual_Integrator.ViewModel;

public interface ICommand
{
        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter);

        public event EventHandler CanExecuteChanged;
}