using System;
using System.Collections.Generic;

namespace RobocopyGUI.Models
{
    /// <summary>
    /// Represents the result of a Robocopy operation.
    /// </summary>
    public class CopyResult
    {
        /// <summary>
        /// Gets or sets whether the operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the exit code from Robocopy.
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// Gets or sets the exit code description.
        /// </summary>
        public string ExitCodeDescription { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the start time of the operation.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of the operation.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets the duration of the operation.
        /// </summary>
        public TimeSpan Duration => EndTime - StartTime;

        /// <summary>
        /// Gets or sets the total number of files processed.
        /// </summary>
        public long TotalFiles { get; set; }

        /// <summary>
        /// Gets or sets the number of files copied.
        /// </summary>
        public long FilesCopied { get; set; }

        /// <summary>
        /// Gets or sets the number of files skipped.
        /// </summary>
        public long FilesSkipped { get; set; }

        /// <summary>
        /// Gets or sets the number of files that failed.
        /// </summary>
        public long FilesFailed { get; set; }

        /// <summary>
        /// Gets or sets the total number of directories processed.
        /// </summary>
        public long TotalDirectories { get; set; }

        /// <summary>
        /// Gets or sets the number of directories copied.
        /// </summary>
        public long DirectoriesCopied { get; set; }

        /// <summary>
        /// Gets or sets the number of directories that failed.
        /// </summary>
        public long DirectoriesFailed { get; set; }

        /// <summary>
        /// Gets or sets the total bytes copied.
        /// </summary>
        public long BytesCopied { get; set; }

        /// <summary>
        /// Gets or sets the error messages.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the warning messages.
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the full output from Robocopy.
        /// </summary>
        public string FullOutput { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the command that was executed.
        /// </summary>
        public string ExecutedCommand { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the operation was cancelled.
        /// </summary>
        public bool WasCancelled { get; set; }

        /// <summary>
        /// Gets the formatted bytes copied as a human-readable string.
        /// </summary>
        public string FormattedBytesCopied => FormatBytes(BytesCopied);

        /// <summary>
        /// Gets a summary of the operation.
        /// </summary>
        public string Summary
        {
            get
            {
                if (WasCancelled)
                {
                    return "Operation was cancelled by user.";
                }

                return $"Copied {FilesCopied} of {TotalFiles} files ({FormattedBytesCopied}) in {Duration:hh\\:mm\\:ss}. " +
                       $"Skipped: {FilesSkipped}, Failed: {FilesFailed}";
            }
        }

        /// <summary>
        /// Gets the description for a Robocopy exit code.
        /// </summary>
        /// <param name="exitCode">The exit code.</param>
        /// <returns>A description of the exit code.</returns>
        public static string GetExitCodeDescription(int exitCode)
        {
            return exitCode switch
            {
                0 => "No files were copied. No failure was encountered. No files were mismatched.",
                1 => "All files were copied successfully.",
                2 => "Extra files or directories detected. Examine the log for details.",
                3 => "Some files were copied. Additional files were present. No failure was encountered.",
                4 => "Mismatched files or directories detected. Examine the log for details.",
                5 => "Some files were copied. Some files were mismatched. No failure was encountered.",
                6 => "Additional files and mismatched files exist. No files were copied and no failures were encountered.",
                7 => "Files were copied, a file mismatch was present, and additional files were present.",
                8 => "Several files did not copy.",
                >= 9 and <= 15 => "Some files were copied but failures occurred.",
                16 => "Serious error. No files were copied.",
                _ => $"Unknown exit code: {exitCode}"
            };
        }

        /// <summary>
        /// Determines if the exit code indicates success.
        /// </summary>
        /// <param name="exitCode">The exit code.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public static bool IsSuccessExitCode(int exitCode)
        {
            // Exit codes 0-7 are considered successful for Robocopy
            return exitCode >= 0 && exitCode <= 7;
        }

        /// <summary>
        /// Formats bytes as a human-readable string.
        /// </summary>
        /// <param name="bytes">The number of bytes.</param>
        /// <returns>A formatted string (e.g., "1.5 GB").</returns>
        private static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB" };
            int suffixIndex = 0;
            double size = bytes;

            while (size >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                size /= 1024;
                suffixIndex++;
            }

            return $"{size:F2} {suffixes[suffixIndex]}";
        }
    }
}
