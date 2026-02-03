using System;
using System.IO;
using System.Text;

namespace RobocopyGUI.Services
{
    /// <summary>
    /// Service for managing application logs.
    /// </summary>
    public class LoggingService
    {
        private readonly string _logDirectory;
        private readonly object _lockObject = new object();
        private string? _currentLogFile;

        /// <summary>
        /// Event raised when a log entry is written.
        /// </summary>
        public event EventHandler<LogEntry>? LogWritten;

        /// <summary>
        /// Gets or sets the maximum number of log files to keep.
        /// </summary>
        public int MaxLogFiles { get; set; } = 30;

        /// <summary>
        /// Gets or sets the maximum size of a single log file in bytes.
        /// </summary>
        public long MaxLogFileSize { get; set; } = 10 * 1024 * 1024; // 10 MB

        /// <summary>
        /// Gets or sets whether to include timestamps in log entries.
        /// </summary>
        public bool IncludeTimestamps { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the LoggingService class.
        /// </summary>
        public LoggingService()
        {
            _logDirectory = GetLogDirectory();
            EnsureLogDirectoryExists();
        }

        /// <summary>
        /// Initializes a new instance of the LoggingService class with a custom log directory.
        /// </summary>
        /// <param name="logDirectory">The directory to store log files.</param>
        public LoggingService(string logDirectory)
        {
            _logDirectory = logDirectory;
            EnsureLogDirectoryExists();
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Info(string message)
        {
            Log(LogLevel.Info, message);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Warning(string message)
        {
            Log(LogLevel.Warning, message);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Error(string message)
        {
            Log(LogLevel.Error, message);
        }

        /// <summary>
        /// Logs an error message with exception details.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">The exception to log.</param>
        public void Error(string message, Exception exception)
        {
            Log(LogLevel.Error, $"{message}\nException: {exception.Message}\nStack Trace: {exception.StackTrace}");
        }

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Debug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        /// <summary>
        /// Logs a message with the specified log level.
        /// </summary>
        /// <param name="level">The log level.</param>
        /// <param name="message">The message to log.</param>
        public void Log(LogLevel level, string message)
        {
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message
            };

            WriteToFile(entry);
            LogWritten?.Invoke(this, entry);
        }

        /// <summary>
        /// Starts a new log file for a copy operation.
        /// </summary>
        /// <param name="operationName">The name of the operation.</param>
        /// <returns>The path to the new log file.</returns>
        public string StartNewLogFile(string operationName)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var safeName = SanitizeFileName(operationName);
            _currentLogFile = Path.Combine(_logDirectory, $"{timestamp}_{safeName}.log");
            
            Info($"Started new log file: {_currentLogFile}");
            
            return _currentLogFile;
        }

        /// <summary>
        /// Gets the current log file path.
        /// </summary>
        /// <returns>The current log file path, or null if no log file is active.</returns>
        public string? GetCurrentLogFile()
        {
            return _currentLogFile;
        }

        /// <summary>
        /// Exports logs to a file.
        /// </summary>
        /// <param name="filePath">The path to export logs to.</param>
        /// <param name="startDate">The start date for log filtering.</param>
        /// <param name="endDate">The end date for log filtering.</param>
        public void ExportLogs(string filePath, DateTime? startDate = null, DateTime? endDate = null)
        {
            var sb = new StringBuilder();
            var logFiles = Directory.GetFiles(_logDirectory, "*.log");

            foreach (var logFile in logFiles)
            {
                var fileDate = File.GetCreationTime(logFile);
                
                if (startDate.HasValue && fileDate < startDate.Value)
                    continue;
                    
                if (endDate.HasValue && fileDate > endDate.Value)
                    continue;

                sb.AppendLine($"=== {Path.GetFileName(logFile)} ===");
                sb.AppendLine(File.ReadAllText(logFile));
                sb.AppendLine();
            }

            File.WriteAllText(filePath, sb.ToString());
        }

        /// <summary>
        /// Cleans up old log files based on the MaxLogFiles setting.
        /// </summary>
        public void CleanupOldLogs()
        {
            var logFiles = Directory.GetFiles(_logDirectory, "*.log");
            
            if (logFiles.Length <= MaxLogFiles)
                return;

            // Sort by creation time and delete oldest
            Array.Sort(logFiles, (a, b) => File.GetCreationTime(a).CompareTo(File.GetCreationTime(b)));

            var filesToDelete = logFiles.Length - MaxLogFiles;
            for (int i = 0; i < filesToDelete; i++)
            {
                try
                {
                    File.Delete(logFiles[i]);
                }
                catch
                {
                    // Ignore deletion errors
                }
            }
        }

        /// <summary>
        /// Gets all log files.
        /// </summary>
        /// <returns>An array of log file paths.</returns>
        public string[] GetLogFiles()
        {
            return Directory.GetFiles(_logDirectory, "*.log");
        }

        /// <summary>
        /// Gets the log directory path.
        /// </summary>
        /// <returns>The log directory path.</returns>
        public string GetLogDirectoryPath()
        {
            return _logDirectory;
        }

        /// <summary>
        /// Gets the default log directory.
        /// </summary>
        /// <returns>The default log directory path.</returns>
        private static string GetLogDirectory()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appData, "RobocopyGUI", "Logs");
        }

        /// <summary>
        /// Ensures the log directory exists.
        /// </summary>
        private void EnsureLogDirectoryExists()
        {
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        /// <summary>
        /// Writes a log entry to the current log file.
        /// </summary>
        /// <param name="entry">The log entry to write.</param>
        private void WriteToFile(LogEntry entry)
        {
            if (string.IsNullOrEmpty(_currentLogFile))
            {
                StartNewLogFile("General");
            }

            lock (_lockObject)
            {
                try
                {
                    // Check if we need to rotate the log file
                    if (File.Exists(_currentLogFile))
                    {
                        var fileInfo = new FileInfo(_currentLogFile);
                        if (fileInfo.Length >= MaxLogFileSize)
                        {
                            StartNewLogFile("General");
                        }
                    }

                    var line = FormatLogEntry(entry);
                    File.AppendAllText(_currentLogFile!, line + Environment.NewLine);
                }
                catch
                {
                    // Ignore file write errors
                }
            }
        }

        /// <summary>
        /// Formats a log entry as a string.
        /// </summary>
        /// <param name="entry">The log entry to format.</param>
        /// <returns>The formatted log entry string.</returns>
        private string FormatLogEntry(LogEntry entry)
        {
            var sb = new StringBuilder();

            if (IncludeTimestamps)
            {
                sb.Append($"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss}] ");
            }

            sb.Append($"[{entry.Level}] ");
            sb.Append(entry.Message);

            return sb.ToString();
        }

        /// <summary>
        /// Sanitizes a file name by removing invalid characters.
        /// </summary>
        /// <param name="fileName">The file name to sanitize.</param>
        /// <returns>The sanitized file name.</returns>
        private static string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new StringBuilder(fileName);
            
            foreach (var c in invalidChars)
            {
                sanitized.Replace(c, '_');
            }
            
            return sanitized.ToString();
        }
    }

    /// <summary>
    /// Represents a log entry.
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Gets or sets the timestamp of the log entry.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the log level.
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// Gets or sets the log message.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Defines the log levels.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>Debug level for detailed diagnostic information.</summary>
        Debug,
        /// <summary>Info level for general information.</summary>
        Info,
        /// <summary>Warning level for potentially harmful situations.</summary>
        Warning,
        /// <summary>Error level for error events.</summary>
        Error
    }
}
