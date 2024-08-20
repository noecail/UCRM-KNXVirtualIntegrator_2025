using System.IO;
using System.Xml.Serialization;
using KNX_Virtual_Integrator.Model.Interfaces;

namespace KNX_Virtual_Integrator.Model.Implementations
{
    /// <summary>
    /// Provides an implementation for managing application settings, including reading from and writing to an XML file.
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

        private readonly string _settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.xml");

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationSettings"/> class.
        /// </summary>
        /// <remarks>
        /// Loads the settings from the XML file if it exists. If the file is not found or an error occurs during loading,
        /// the default settings are used.
        /// </remarks>
        public ApplicationSettings(ApplicationFileManager manager, SystemSettingsDetector detector)
        {
            // Si le fichier de paramétrage n'existe pas, on détecte les paramètres de windows
            if (!manager.EnsureSettingsFileExists(_settingsFilePath))
            {
                Console.WriteLine("Les paramètres existent pas on détecte");
                AppLang = detector.DetectWindowsLanguage();
                EnableLightTheme = detector.DetectWindowsTheme();
            }
            else
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(ApplicationSettings));
                    using var fileStream = new FileStream(_settingsFilePath, FileMode.Open);
                    var loadedSettings = (ApplicationSettings)serializer.Deserialize(fileStream)!;

                    EnableLightTheme = loadedSettings.EnableLightTheme;
                    AppLang = loadedSettings.AppLang;
                    AppScaleFactor = loadedSettings.AppScaleFactor;
                    
                    Console.WriteLine($"Les paramètres existaient, on a détecté: {EnableLightTheme}, {AppLang}, {AppScaleFactor}");
                }
                catch (Exception e)
                {
                    // Handle the exception (e.g., log the error, notify the user, etc.)
                    Console.WriteLine($"Error loading settings: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Parameterless constructor required for XML serialization
        /// </summary>
        public ApplicationSettings()
        {
            
        }

        /// <summary>
        /// Saves the current settings to an XML file.
        /// </summary>
        /// <remarks>
        /// Serializes the settings and writes them to the XML file. If an error occurs during the saving process, 
        /// the error should be handled accordingly.
        /// </remarks>
        public void Save()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(ApplicationSettings));
                using var fileStream = new FileStream(_settingsFilePath, FileMode.Create);
                serializer.Serialize(fileStream, this);
            }
            catch (Exception e)
            {
                // Handle the exception (e.g., log the error, notify the user, etc.)
                Console.WriteLine($"Error saving settings: {e.Message}");
            }
        }
    }
}
