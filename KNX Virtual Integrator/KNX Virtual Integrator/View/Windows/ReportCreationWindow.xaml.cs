using System.Windows;
using System.Windows.Input;
using KNX_Virtual_Integrator.ViewModel;

namespace KNX_Virtual_Integrator.View.Windows;

/// <summary>
/// Represents the window for creating reports.
/// </summary>
public partial class ReportCreationWindow
{
    private readonly MainViewModel _mainViewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportCreationWindow"/> class.
    /// </summary>
    /// <param name="mv">The view model associated with the window.</param>
    public ReportCreationWindow(MainViewModel mv)
    {
        InitializeComponent();
        _mainViewModel = mv;
    }

    /// <summary>
    /// Handles the MouseLeftButtonDown event to initiate dragging of the window.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;
        DragMove();
    }

    /// <summary>
    /// Handles the event to close (hide) the report creation window.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void CloseReportCreationWindow(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    /// <summary>
    /// Handles the click event of the save button to generate a report.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void SaveButtonClick(object sender, RoutedEventArgs e)
    {
        _mainViewModel.GenerateReportCommand.Execute(("test.pdf", authorNameTextBox.Text.Trim()));
    }

    /// <summary>
    /// Handles the click event of the cancel button. This method is not yet implemented.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    private void CancelButtonClick(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}