namespace KNX_Virtual_Integrator.Model.Interfaces
{
    /// <summary>
    /// Defines methods for detecting system settings related to Windows, such as theme and language.
    /// </summary>
    public interface ISystemSettingsDetector
    {
        /// <summary>
        /// Detects the current Windows theme (light or dark).
        /// Attempts to read the theme setting from the Windows registry.
        /// Returns true if the theme is light, false if it is dark.
        /// If an error occurs or the registry value is not found, defaults to true (light theme).
        /// </summary>
        /// <returns>
        /// A boolean value indicating whether the Windows theme is light (true) or dark (false).
        /// </returns>
        bool DetectWindowsTheme();

        /// <summary>
        /// Detects the current Windows language.
        /// If the language is supported by the application, it returns the corresponding language code.
        /// Otherwise, it returns an empty string.
        /// </summary>
        /// <returns>
        /// A string representing the Windows language code if supported; otherwise, an empty string.
        /// </returns>
        /// <remarks>
        /// This method reads the "LocaleName" value from the Windows registry under "Control Panel\International".
        /// It extracts the language code from this value and checks if it is in the set of valid language codes.
        /// If an error occurs during the registry access or if the language code is not supported, an empty string is returned.
        /// </remarks>
        string DetectWindowsLanguage();

        // Fonction permettant de détecter le thème de windows (clair/sombre)
        /// <summary>
        /// Detects the current Windows scale (100%,125%,...).
        /// Attempts to read the scale setting from the Windows registry.
        /// Returns the value : 100% returns 100.
        /// If an error occurs or the registry value is not found, defaults to 100% .
        /// </summary>
        /// <returns>
        /// An integer value indicating the System scale.
        /// </returns>
        int DetectWindowsScale();
    }
}