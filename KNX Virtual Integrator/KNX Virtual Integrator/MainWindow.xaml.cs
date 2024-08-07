using System.Windows.Media;

namespace KNX_Virtual_Integrator;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }
    
    
    /// <summary>
    /// Converts a string representation of a color to a SolidColorBrush.
    /// </summary>
    /// <param name="colorInput">The string representation of the color (e.g., "#RRGGBB" or "ColorName").</param>
    /// <returns>A SolidColorBrush representing the converted color.</returns>
    public static SolidColorBrush ConvertStringColor(string colorInput)
    {
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorInput));
    }

    public void ApplyScaling(float scaleFactor)
    {
        App.ConsoleAndLogWriteLine("MainWindow.ApplyScaling is not implemented");
    }

    public void UpdateWindowContents(bool b, bool b1, bool b2)
    {
        App.ConsoleAndLogWriteLine("MainWindow.UpdateWindowContents is not implemented");
    }
}