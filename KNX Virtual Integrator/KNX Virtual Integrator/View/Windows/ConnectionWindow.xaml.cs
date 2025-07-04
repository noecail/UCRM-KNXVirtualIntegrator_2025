using System.ComponentModel;
using KNX_Virtual_Integrator.ViewModel;

namespace KNX_Virtual_Integrator.View.Windows;

public partial class ConnectionWindow 
{
    public ConnectionWindow(MainViewModel viewModel)
    {
        InitializeComponent();

        DataContext = viewModel;
    }

    private void ClosingConnectionWindow(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
    }
}