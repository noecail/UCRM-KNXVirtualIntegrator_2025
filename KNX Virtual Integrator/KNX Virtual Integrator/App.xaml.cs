namespace KNX_Virtual_Integrator;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    
    // Fonction permettant l'affichage d'un message dans la console de l'application tout en l'ecrivant dans les
    // logs. Ajoute la date et l'heure avant affichage. Saut d'une ligne en fin de message.
    /// <summary>
    /// Writes a message to the application console and log file, including the current date and time, and appends a newline after the message.
    /// <para>
    /// This method performs the following tasks:
    /// <list type="bullet">
    ///     <item>
    ///         Writes the provided message to the console with a timestamp (date and time) at the beginning, followed by a newline.
    ///     </item>
    ///     <item>
    ///         If the console window is visible, scrolls to the end of the console text to ensure that the latest message is displayed.
    ///     </item>
    ///     <item>
    ///         Writes the same message to the log file with a timestamp (date and time) at the beginning, followed by a newline.
    ///     </item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="msg">The message to be written to the console and log file.</param>
    public static void ConsoleAndLogWriteLine(string msg)
    {
        Console.WriteLine($@"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] " + msg); // Ecriture du message dans la console
        
        // TODO : A RAJOUTER UNE FOIS LES LOGS GERES
        //_writer?.WriteLine($"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] " + msg); // Ecriture du message dans le fichier logs
    }
}