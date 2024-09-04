using System.Windows;
using System.Windows.Input;
using KNX_Virtual_Integrator.ViewModel;

namespace KNX_Virtual_Integrator.View.Windows;

public partial class ReportCreationWindow
{
    private readonly MainViewModel _mainViewModel;
    
    public ReportCreationWindow(MainViewModel mv)
    {
        InitializeComponent();
        _mainViewModel = mv;
    }

    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;
        DragMove();
    }

    private void CloseReportCreationWindow(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private void SaveButtonClick(object sender, RoutedEventArgs e)
    {
        _mainViewModel.GenerateReportCommand.Execute(("test.pdf",authorNameTextBox.Text.Trim()));
    }

    private void CancelButtonClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}