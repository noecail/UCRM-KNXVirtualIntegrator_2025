using System.IO;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace KNX_Virtual_Integrator.Model.Implementations;

// TODO: Créer l'interface associée + <summary>

public class ApplicationSettings
{
    public bool EnableLightTheme { get; set; } = true;
    public string AppLang { get; set; } = "FR";
    public int AppScaleFactor { get; set; } = 100;

    private readonly string _settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

    public ApplicationSettings Load()
    {
        if (!File.Exists(_settingsFilePath))
            return new ApplicationSettings(); // Charger les paramètres par défaut si le fichier n'existe pas
        
        try
        {
            var json = File.ReadAllText(_settingsFilePath);
            return JsonConvert.DeserializeObject<ApplicationSettings>(json) ?? new ApplicationSettings();
        }
        catch (Exception)
        {
            return new ApplicationSettings(); // Charger les paramètres par défaut en cas d'échec
        }
    }

    public void Save()
    {
        try
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(_settingsFilePath, json);
        }
        catch (Exception e)
        {
            // Gérer les erreurs de sauvegarde ici
        }
    }
}