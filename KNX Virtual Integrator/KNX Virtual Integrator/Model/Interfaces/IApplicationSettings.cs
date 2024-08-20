namespace KNX_Virtual_Integrator.Model.Interfaces
{
    /// <summary>
    /// Defines the interface for application settings management.
    /// </summary>
    public interface IApplicationSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether the light theme is enabled.
        /// </summary>
        bool EnableLightTheme { get; set; }

        /// <summary>
        /// Gets or sets the language used by the application.
        /// </summary>
        string AppLang { get; set; }

        /// <summary>
        /// Gets or sets the scale factor of the application interface.
        /// </summary>
        int AppScaleFactor { get; set; }

        /// <summary>
        /// Saves the current application settings.
        /// </summary>
        void Save();
    }
}