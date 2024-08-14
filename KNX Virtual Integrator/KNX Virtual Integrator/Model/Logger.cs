using System.IO;

namespace KNX_Virtual_Integrator.Model;

/// <summary>
/// Provides logging functionality for the application by writing log entries to a specified log file.
/// <para>
/// The <see cref="Logger"/> class allows messages to be recorded in a log file, optionally including timestamps, 
/// and provides methods for both console output combined with logging and logging to the file only.
/// </para>
/// <para>
/// The log file is automatically named with the current date and time to prevent overwriting previous logs.
/// </para>
/// </summary>
public class Logger
{
    /* ------------------------------------------------------------------------------------------------
    ------------------------------------------- ATTRIBUTS  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    /// <summary>
    /// Stores the file path for the log file. This path is used to determine where the log entries will be written.
    /// </summary>
    public static string? LogPath { get; } = $"./logs/logs-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt"; // Chemin du fichier logs
        
    /// <summary>
    /// Provides a <see cref="StreamWriter"/> instance for writing log entries to the log file.
    /// </summary>
    /// <remarks>
    /// This writer is used for appending log messages to the file specified by <see cref="LogPath"/>.
    /// </remarks>
    public static readonly StreamWriter Writer = new(LogPath); // Permet l'ecriture du fichier de logging
    
    
    
    
    /* ------------------------------------------------------------------------------------------------
    -------------------------------------------- METHODES  --------------------------------------------
    ------------------------------------------------------------------------------------------------ */
    // Fonction permettant l'affichage d'un message dans la console de l'application tout en l'ecrivant dans les
    // logs sans sauter de ligne apres le message.
    /// <summary>
    /// Writes a message to the application console and log file without appending a newline after the message.
    /// <para>
    /// This method performs the following tasks:
    /// <list type="bullet">
    ///     <item>
    ///         Writes the provided message to the console without adding a newline character.
    ///     </item>
    ///     <item>
    ///         If the console window is visible, scrolls to the end of the console text to ensure the latest message is visible.
    ///     </item>
    ///     <item>
    ///         Writes the same message to the log file without appending a newline character.
    ///     </item>
    /// </list>
    /// </para>
    /// </summary>
    /// <param name="msg">The message to be written to the console and log file.</param>
    public static void ConsoleAndLogWrite(string msg)
    {
        Console.Write(msg); // Ecriture du message dans la console
        Writer.Write(msg); // Ecriture du message dans le fichier logs
    }

    
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
        Console.WriteLine($"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] " + msg); // Ecriture du message dans la console
        Writer.WriteLine($"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] " + msg); // Ecriture du message dans le fichier logs
    }
    
    
    /// <summary>
    /// Writes a message to the log file without appending a newline after the message.
    /// </summary>
    /// <param name="msg">The message to be written to the log file.</param>
    public static void LogWrite(string msg)
    {
        Writer.Write(msg); // Ecriture du message dans le fichier logs
    }

    
    /// <summary>
    /// Writes a message to the log file, including the current date and time, and appends a newline after the message.
    /// </summary>
    /// <param name="msg">The message to be written to the log file.</param>
    public static void LogWriteLine(string msg)
    {
        Writer.WriteLine($"[{DateTime.Now:dd/MM/yyyy - HH:mm:ss}] " + msg); // Ecriture du message avec timestamp dans le fichier logs
    }
}