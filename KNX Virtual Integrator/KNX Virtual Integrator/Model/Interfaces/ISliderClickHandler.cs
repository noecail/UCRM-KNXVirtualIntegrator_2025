using System.Windows;
using System.Windows.Input;

namespace KNX_Virtual_Integrator.Model.Interfaces
{
    public interface ISliderClickHandler
    {
        /// <summary>
        /// Handles the click event of a slider's RepeatButton, updating the slider's value based on the click position.
        /// </summary>
        void OnSliderClick(object sender, RoutedEventArgs e);

        /// <summary>
        /// Handles the event when the left mouse button is pressed down on the slider.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data for the mouse button event.</param>
        void SliderMouseLeftButtonDown(object sender, MouseButtonEventArgs e);

        /// <summary>
        /// Handles the event when the left mouse button is released on the slider.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data for the mouse button event.</param>
        void SliderMouseLeftButtonUp(object sender, MouseButtonEventArgs e);

        /// <summary>
        /// Handles the event when the mouse is moved over the slider while dragging.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data for the mouse movement event.</param>
        void SliderMouseMove(object sender, MouseEventArgs e);

        /// <summary>
        /// Updates the slider's value based on the current mouse position.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data for the mouse movement event.</param>
        void UpdateSliderValue(object sender, MouseEventArgs e);
    }
}