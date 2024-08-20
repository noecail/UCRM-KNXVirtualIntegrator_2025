using System.IO;
using KNX_Virtual_Integrator.Model.Interfaces;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace KNX_Virtual_Integrator.Model.Implementations
{
    /// <summary>
    /// Provides an implementation for managing application settings, including reading from and writing to a JSON file.
    /// </summary>
    public class ApplicationSettings : IApplicationSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether the light theme is enabled.
        /// </summary>
        public bool EnableLightTheme { get; set; } = true;

        /// <summary>
        /// Gets or sets the language used by the application.
        /// </summary>
        public string AppLang { get; set; } = "FR";

        /// <summary>
        /// Gets or sets the scale factor of the application interface.
        /// </summary>
        public int AppScaleFactor { get; set; } = 100;

        private readonly string _settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationSettings"/> class.
        /// </summary>
        /// <remarks>
        /// Loads the settings from the JSON file if it exists. If the file is not found or an error occurs during loading,
        /// the default settings are used.
        /// </remarks>
        public ApplicationSettings()
        {
            if (!File.Exists(_settingsFilePath)) return;

            try
            {
                var json = File.ReadAllText(_settingsFilePath);
                var loadedSettings = JsonConvert.DeserializeObject<ApplicationSettings>(json);

                if (loadedSettings == null) return;

                EnableLightTheme = loadedSettings.EnableLightTheme;
                AppLang = loadedSettings.AppLang;
                AppScaleFactor = loadedSettings.AppScaleFactor;
            }
            catch (Exception e)
            {
                // A FAIRE
            }
        }

        /// <summary>
        /// Saves the current settings to a JSON file.
        /// </summary>
        /// <remarks>
        /// Serializes the settings and writes them to the JSON file. If an error occurs during the saving process, 
        /// the error should be handled accordingly.
        /// </remarks>
        public void Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception e)
            {
                // A FAIRE
            }
        }
    }
}
