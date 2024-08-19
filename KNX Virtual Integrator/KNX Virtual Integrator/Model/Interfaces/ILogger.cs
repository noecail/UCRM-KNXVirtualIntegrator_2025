namespace KNX_Virtual_Integrator.Model.Interfaces
{
    /// <summary>
    /// Defines the contract for logging functionality.
    /// <para>
    /// The <see cref="ILogger"/> interface provides methods for writing log messages to both the console and a log file.
    /// It supports writing messages with or without timestamps, and allows for both single-line and multi-line messages.
    /// </para>
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Writes a message to the application console and log file without appending a newline after the message.
        /// </summary>
        /// <param name="msg">The message to be written to the console and log file.</param>
        void ConsoleAndLogWrite(string msg);

        /// <summary>
        /// Writes a message to the application console and log file, including the current date and time, and appends a newline after the message.
        /// </summary>
        /// <param name="msg">The message to be written to the console and log file.</param>
        void ConsoleAndLogWriteLine(string msg);

        /// <summary>
        /// Writes a message to the log file without appending a newline after the message.
        /// </summary>
        /// <param name="msg">The message to be written to the log file.</param>
        void LogWrite(string msg);

        /// <summary>
        /// Writes a message to the log file, including the current date and time, and appends a newline after the message.
        /// </summary>
        /// <param name="msg">The message to be written to the log file.</param>
        void LogWriteLine(string msg);

        /// <summary>
        /// Closes the log writer and releases any resources associated with it.
        /// </summary>
        void CloseLogWriter();
    }
}