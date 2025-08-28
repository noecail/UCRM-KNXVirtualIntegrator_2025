using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using KNX_Virtual_Integrator.Model.Interfaces;

namespace KNX_Virtual_Integrator.Model.Implementations;
/// <summary>
/// Class handling the manipulation of the slider in the <see cref="View.Windows.SettingsWindow"/>.
/// </summary>
/// <param name="logger">To log errors and notable events.</param>
/// <param name="parentFinder">To find the visual parent of the current object</param>
public class SliderClickHandler (ILogger logger, ParentFinder parentFinder) : ISliderClickHandler
{
    private bool _isDragging;
    
    /// <summary>
    /// Handles the click event of a slider's RepeatButton, updating the slider's value based on the click position.
    /// </summary>
    public void OnSliderClick(object sender, RoutedEventArgs e)
    {
        try
        {
            // Vérifie si l'objet sender est un RepeatButton
            if (sender is not RepeatButton repeatButton) return;
        
            // Trouve le Slider parent du RepeatButton
            var slider = parentFinder.FindParent<Slider>(repeatButton);
        
            // Si le slider est null, quitter la méthode
            if (slider == null) 
            {
                logger.ConsoleAndLogWriteLine("Slider not found.");
                return;
            }
        
            // Obtient la position de la souris relative au slider
            var position = Mouse.GetPosition(slider);
        
            // Calcule la nouvelle valeur du slider en fonction de la position de la souris
            var relativeX = position.X / slider.ActualWidth;
            var newValue = slider.Minimum + (relativeX * (slider.Maximum - slider.Minimum));
        
            // Met à jour la valeur du slider
            slider.Value = newValue;
        }
        catch (Exception ex)
        {
            // Logue l'erreur en cas d'exception
            logger.ConsoleAndLogWriteLine($"An error occurred: {ex.Message}");
        }
    }


    /// <summary>
    /// Handles the event when the left mouse button is pressed down on the slider.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event data for the mouse button event.</param>
    public void SliderMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        try
        {
            // Indique que le glissement est en cours
            _isDragging = true;

            if (sender is Slider slider)
            {
                // Capture le mouse pour suivre le mouvement au-dessus du slider
                Mouse.Capture(slider);
            }
        
            // Met à jour la valeur du slider en fonction de la position de la souris
            UpdateSliderValue(sender, e);
            
        }
        catch (Exception ex)
        {
            // Log l'erreur en cas d'exception
            logger.ConsoleAndLogWriteLine($"An error occurred in SliderMouseLeftButtonDown: {ex.Message}");
        }
    }

        
    /// <summary>
    /// Handles the event when the left mouse button is released on the slider.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event data for the mouse button event.</param>
    public void SliderMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        try
        {
            // Indique que le glissement est terminé
            _isDragging = false;
        
            // Relâche la capture de la souris
            Mouse.Capture(null);
        }
        catch (Exception ex)
        {
            // Log l'erreur en cas d'exception
            logger.ConsoleAndLogWriteLine($"An error occurred in SliderMouseLeftButtonUp: {ex.Message}");
        }
    }

        
    /// <summary>
    /// Handles the event when the mouse is moved over the slider while dragging.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event data for the mouse movement event.</param>
    public void SliderMouseMove(object sender, MouseEventArgs e)
    {
        try
        {
            // Met à jour la valeur du slider uniquement si le glissement est en cours
            if (_isDragging)
            {
                UpdateSliderValue(sender, e);
            }
        }
        catch (Exception ex)
        {
            // Log l'erreur en cas d'exception
            logger.ConsoleAndLogWriteLine($"An error occurred in SliderMouseMove: {ex.Message}");
        }
    }

        
    /// <summary>
    /// Updates the slider's value based on the current mouse position.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event data for the mouse movement event.</param>
    public void UpdateSliderValue(object sender, MouseEventArgs e)
    {
        try
        {
            // Vérifie si l'objet sender est un Slider
            if (sender is not Slider slider) return;
        
            // Obtient la position de la souris relative au slider
            var position = e.GetPosition(slider);
        
            // Calcule la nouvelle valeur du slider en fonction de la position de la souris
            var relativeX = position.X / slider.ActualWidth;
            var newValue = slider.Minimum + (relativeX * (slider.Maximum - slider.Minimum));
        
            // Ajuste la valeur pour correspondre au tick le plus proche
            var tickFrequency = slider.TickFrequency;
            newValue = Math.Round(newValue / tickFrequency) * tickFrequency;
        
            // Met à jour la valeur du slider en s'assurant qu'elle reste dans les limites
            slider.Value = Math.Max(slider.Minimum, Math.Min(slider.Maximum, newValue));
        }
        catch (Exception ex)
        {
            // Log l'erreur en cas d'exception
            logger.ConsoleAndLogWriteLine($"An error occurred in UpdateSliderValue: {ex.Message}");
        }
    }
}