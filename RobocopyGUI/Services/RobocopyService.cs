using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RobocopyGUI.Models;

namespace RobocopyGUI.Services
{
    /// <summary>
    /// Service for building and executing Robocopy commands.
    /// </summary>
    public class RobocopyService
    {
        private Process? _currentProcess;
        private CancellationTokenSource? _cancellationTokenSource;

        /// <summary>
        /// Event raised when output is received from the Robocopy process.
        /// </summary>
        public event EventHandler<string>? OutputReceived;

        /// <summary>
        /// Event raised when an error is received from the Robocopy process.
        /// </summary>
        public event EventHandler<string>? ErrorReceived;

        /// <summary>
        /// Event raised when the copy operation is completed.
        /// </summary>
        public event EventHandler<CopyResult>? CopyCompleted;

        /// <summary>
        /// Gets whether a copy operation is currently running.
        /// </summary>
        public bool IsRunning => _currentProcess != null && !_currentProcess.HasExited;

        /// <summary>
        /// Builds the Robocopy command arguments from the given options.
        /// </summary>
        /// <param name="options">The copy options.</param>
        /// <returns>The command line arguments string.</returns>
        public string BuildCommand(CopyOptions options)
        {
            var args = new List<string>();

            // Source and destination paths (quoted for spaces)
            args.Add($"\"{options.SourcePath}\"");
            args.Add($"\"{options.DestinationPath}\"");

            // Mode-specific options
            switch (options.Mode)
            {
                case CopyMode.Mirror:
                    args.Add("/MIR");
                    break;
                case CopyMode.Backup:
                    if (options.CopySubdirectories)
                    {
                        args.Add(options.IncludeEmptyDirectories ? "/E" : "/S");
                    }
                    break;
                case CopyMode.Sync:
                    args.Add("/MIR");
                    args.Add("/IT"); // Include tweaked files
                    break;
                case CopyMode.Move:
                    args.Add("/MOVE");
                    if (options.CopySubdirectories)
                    {
                        args.Add(options.IncludeEmptyDirectories ? "/E" : "/S");
                    }
                    break;
            }

            // Multi-threading
            if (options.ThreadCount > 1)
            {
                args.Add($"/MT:{options.ThreadCount}");
            }

            // Retry options
            args.Add($"/R:{options.RetryCount}");
            args.Add($"/W:{options.RetryWaitTime}");

            // Restartable mode
            if (options.RestartableMode)
            {
                args.Add("/Z");
            }

            // Backup mode (requires admin)
            if (options.BackupMode)
            {
                args.Add("/B");
            }

            // Copy attributes
            if (options.CopyAttributes)
            {
                args.Add("/COPY:DAT"); // Data, Attributes, Timestamps
            }

            // Verbose output
            if (options.VerboseOutput)
            {
                args.Add("/V");
            }
            else
            {
                // Standard minimal output for performance
                args.Add("/NP"); // No progress percentage
            }

            // Include patterns
            if (options.IncludePatterns.Count > 0)
            {
                foreach (var pattern in options.IncludePatterns)
                {
                    args.Add($"\"{pattern}\"");
                }
            }

            // Exclude patterns
            if (options.ExcludePatterns.Count > 0)
            {
                args.Add("/XF");
                foreach (var pattern in options.ExcludePatterns)
                {
                    args.Add($"\"{pattern}\"");
                }
            }

            // Exclude directories
            if (options.ExcludeDirectories.Count > 0)
            {
                args.Add("/XD");
                foreach (var dir in options.ExcludeDirectories)
                {
                    args.Add($"\"{dir}\"");
                }
            }

            // Custom parameters
            if (!string.IsNullOrWhiteSpace(options.CustomParameters))
            {
                args.Add(options.CustomParameters);
            }

            return string.Join(" ", args);
        }

        /// <summary>
        /// Gets the full command line that would be executed.
        /// </summary>
        /// <param name="options">The copy options.</param>
        /// <returns>The full command line string.</returns>
        public string GetFullCommand(CopyOptions options)
        {
            return $"robocopy {BuildCommand(options)}";
        }

        /// <summary>
        /// Executes the copy operation asynchronously.
        /// </summary>
        /// <param name="options">The copy options.</param>
        /// <returns>The result of the copy operation.</returns>
        public async Task<CopyResult> ExecuteAsync(CopyOptions options)
        {
            var result = new CopyResult
            {
                StartTime = DateTime.Now,
                ExecutedCommand = GetFullCommand(options)
            };

            var outputBuilder = new StringBuilder();
            _cancellationTokenSource = new CancellationTokenSource();

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "robocopy",
                    Arguments = BuildCommand(options),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                _currentProcess = new Process { StartInfo = startInfo };

                _currentProcess.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        outputBuilder.AppendLine(e.Data);
                        OutputReceived?.Invoke(this, e.Data);
                    }
                };

                _currentProcess.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        result.Errors.Add(e.Data);
                        ErrorReceived?.Invoke(this, e.Data);
                    }
                };

                _currentProcess.Start();
                _currentProcess.BeginOutputReadLine();
                _currentProcess.BeginErrorReadLine();

                // Wait for process to exit or cancellation
                await Task.Run(() =>
                {
                    while (!_currentProcess.HasExited)
                    {
                        if (_cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            _currentProcess.Kill(true);
                            result.WasCancelled = true;
                            break;
                        }
                        Thread.Sleep(100);
                    }
                });

                if (!result.WasCancelled)
                {
                    await _currentProcess.WaitForExitAsync();
                }

                result.ExitCode = _currentProcess.ExitCode;
                result.ExitCodeDescription = CopyResult.GetExitCodeDescription(result.ExitCode);
                result.Success = CopyResult.IsSuccessExitCode(result.ExitCode);
                result.FullOutput = outputBuilder.ToString();

                // Parse statistics from output
                ParseStatistics(result);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add($"Error executing Robocopy: {ex.Message}");
            }
            finally
            {
                result.EndTime = DateTime.Now;
                _currentProcess?.Dispose();
                _currentProcess = null;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;

                CopyCompleted?.Invoke(this, result);
            }

            return result;
        }

        /// <summary>
        /// Cancels the current copy operation.
        /// </summary>
        public void Cancel()
        {
            _cancellationTokenSource?.Cancel();
        }

        /// <summary>
        /// Parses copy statistics from the Robocopy output.
        /// </summary>
        /// <param name="result">The result to update with statistics.</param>
        private void ParseStatistics(CopyResult result)
        {
            // Parse output for statistics
            var lines = result.FullOutput.Split('\n');
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Parse file statistics
                if (trimmedLine.StartsWith("Files :"))
                {
                    ParseFileLine(trimmedLine, result);
                }
                // Parse directory statistics
                else if (trimmedLine.StartsWith("Dirs :"))
                {
                    ParseDirLine(trimmedLine, result);
                }
                // Parse bytes statistics
                else if (trimmedLine.StartsWith("Bytes :"))
                {
                    ParseBytesLine(trimmedLine, result);
                }
            }
        }

        /// <summary>
        /// Parses the Files line from Robocopy output.
        /// </summary>
        private void ParseFileLine(string line, CopyResult result)
        {
            try
            {
                // Format: "Files :    10    5    3    2    0    0"
                // Columns: Total, Copied, Skipped, Mismatch, Failed, Extras
                var parts = line.Replace("Files :", "").Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 5)
                {
                    result.TotalFiles = long.TryParse(parts[0], out var total) ? total : 0;
                    result.FilesCopied = long.TryParse(parts[1], out var copied) ? copied : 0;
                    result.FilesSkipped = long.TryParse(parts[2], out var skipped) ? skipped : 0;
                    result.FilesFailed = long.TryParse(parts[4], out var failed) ? failed : 0;
                }
            }
            catch
            {
                // Ignore parsing errors
            }
        }

        /// <summary>
        /// Parses the Dirs line from Robocopy output.
        /// </summary>
        private void ParseDirLine(string line, CopyResult result)
        {
            try
            {
                var parts = line.Replace("Dirs :", "").Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 5)
                {
                    result.TotalDirectories = long.TryParse(parts[0], out var total) ? total : 0;
                    result.DirectoriesCopied = long.TryParse(parts[1], out var copied) ? copied : 0;
                    result.DirectoriesFailed = long.TryParse(parts[4], out var failed) ? failed : 0;
                }
            }
            catch
            {
                // Ignore parsing errors
            }
        }

        /// <summary>
        /// Parses the Bytes line from Robocopy output.
        /// </summary>
        private void ParseBytesLine(string line, CopyResult result)
        {
            try
            {
                var parts = line.Replace("Bytes :", "").Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    // Try to parse bytes copied (second column)
                    if (long.TryParse(parts[1], out var bytes))
                    {
                        result.BytesCopied = bytes;
                    }
                }
            }
            catch
            {
                // Ignore parsing errors
            }
        }

        /// <summary>
        /// Validates the copy options.
        /// </summary>
        /// <param name="options">The options to validate.</param>
        /// <returns>A list of validation errors, empty if valid.</returns>
        public List<string> ValidateOptions(CopyOptions options)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(options.SourcePath))
            {
                errors.Add("Source path is required.");
            }
            else if (!System.IO.Directory.Exists(options.SourcePath))
            {
                errors.Add($"Source path does not exist: {options.SourcePath}");
            }

            if (string.IsNullOrWhiteSpace(options.DestinationPath))
            {
                errors.Add("Destination path is required.");
            }

            if (options.ThreadCount < 1 || options.ThreadCount > 128)
            {
                errors.Add("Thread count must be between 1 and 128.");
            }

            if (options.RetryCount < 0)
            {
                errors.Add("Retry count must be 0 or greater.");
            }

            if (options.RetryWaitTime < 0)
            {
                errors.Add("Retry wait time must be 0 or greater.");
            }

            return errors;
        }
    }
}
