using KNX_Virtual_Integrator.Model.Interfaces;
using Microsoft.Win32;

namespace KNX_Virtual_Integrator.Model.Implementations;

public class SystemSettingsDetector (ILogger logger) : ISystemSettingsDetector
{
    // Fonction permettant de détecter le thème de windows (clair/sombre)
    /// <summary>
    /// Detects the current Windows theme (light or dark).
    /// Attempts to read the theme setting from the Windows registry.
    /// Returns true if the theme is light, false if it is dark.
    /// If an error occurs or the registry value is not found, defaults to true (light theme).
    /// </summary>
    /// <returns>
    /// A boolean value indicating whether the Windows theme is light (true) or dark (false).
    /// </returns>
    public bool DetectWindowsTheme()
    {
        try
        {
            // Ouverture de la clé de registre contenant les informations sur le thème Windows.
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
        
            // Récupérer la valeur du registre qui indique si le thème clair est utilisé par les applications.
            var registryValue = key?.GetValue("AppsUseLightTheme");

            // Si la valeur récupérée est un entier, on vérifie si elle est égale à 1.
            // Si c'est le cas, cela signifie que le thème clair est activé.
            if (registryValue is int value)
            {
                return value == 1; // Retourne true si le thème clair est activé, false si le thème sombre est activé.
            }
        }
        catch (Exception ex)
        {
            // En cas d'erreur lors de l'accès à la clé de registre, on écrit sur le log l'erreur avec un message explicatif.
            logger.ConsoleAndLogWriteLine($"Erreur : Une erreur s'est produite lors de la récupération du thème Windows : {ex.Message}. Thème par défaut : clair.");

            // En cas d'erreur, on retourne true, ce qui signifie que le thème clair est utilisé par défaut.
            return true; // Par défaut, on suppose que le thème clair est activé.
        }

        // Si aucune information n'a pu être récupérée, on retourne true, indiquant que le thème clair est utilisé.
        return true;
    }


    
    // Fonction permettant de détecter la langue de Windows. Si elle est supportée par l'application,
    // on retourne le code de la langue correspondante.
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
    public string DetectWindowsLanguage()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\International");
            var registryValue = key?.GetValue("LocaleName");

            if (registryValue != null)
            {
                // Créer un HashSet avec tous les codes de langue pris en charge par la traduction de l'application
                var validLanguageCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "AR", "BG", "CS", "DA", "DE", "EL", "EN", "ES", "ET", "FI",
                    "HU", "ID", "IT", "JA", "KO", "LT", "LV", "NB", "NL", "PL",
                    "PT", "RO", "RU", "SK", "SL", "SV", "TR", "UK", "ZH", "FR"
                };

                var localeName = registryValue.ToString();

                // Extraire les deux premières lettres de localeName pour obtenir le code de langue
                var languageCode = localeName?.Split('-')[0].ToUpper();

                // Vérifier si le code de langue extrait est dans le HashSet
                if (languageCode != null && validLanguageCodes.Contains(languageCode))
                {
                    logger.ConsoleAndLogWriteLine($"Langue windows détectée : {languageCode}");
                    return languageCode;
                }
            }
        }
        catch (Exception ex)
        {
            logger.ConsoleAndLogWriteLine($"Error: An error occured while reading the windows language from registry : {ex.Message}");
            return "";
        }

        return "";
    }
}