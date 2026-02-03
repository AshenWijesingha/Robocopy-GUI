using System;
using System.Collections.Generic;

namespace RobocopyGUI.Models
{
    /// <summary>
    /// Represents the configuration options for a Robocopy operation.
    /// </summary>
    public class CopyOptions
    {
        /// <summary>
        /// Gets or sets the source folder path.
        /// </summary>
        public string SourcePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the destination folder path.
        /// </summary>
        public string DestinationPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the copy mode (Mirror, Backup, Sync, Move).
        /// </summary>
        public CopyMode Mode { get; set; } = CopyMode.Backup;

        /// <summary>
        /// Gets or sets the number of threads for multi-threaded copying.
        /// </summary>
        public int ThreadCount { get; set; } = 8;

        /// <summary>
        /// Gets or sets the number of retries on failed copies.
        /// </summary>
        public int RetryCount { get; set; } = 3;

        /// <summary>
        /// Gets or sets the wait time between retries in seconds.
        /// </summary>
        public int RetryWaitTime { get; set; } = 5;

        /// <summary>
        /// Gets or sets whether to copy subdirectories.
        /// </summary>
        public bool CopySubdirectories { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to copy empty subdirectories.
        /// </summary>
        public bool IncludeEmptyDirectories { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to copy file attributes.
        /// </summary>
        public bool CopyAttributes { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to copy file timestamps.
        /// </summary>
        public bool CopyTimestamps { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to use restartable mode.
        /// </summary>
        public bool RestartableMode { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to use backup mode.
        /// </summary>
        public bool BackupMode { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to show verbose output.
        /// </summary>
        public bool VerboseOutput { get; set; } = false;

        /// <summary>
        /// Gets or sets the file patterns to include (e.g., *.txt, *.doc).
        /// </summary>
        public List<string> IncludePatterns { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the file patterns to exclude.
        /// </summary>
        public List<string> ExcludePatterns { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the directories to exclude.
        /// </summary>
        public List<string> ExcludeDirectories { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets additional custom parameters.
        /// </summary>
        public string CustomParameters { get; set; } = string.Empty;

        /// <summary>
        /// Creates a deep copy of this CopyOptions instance.
        /// </summary>
        /// <returns>A new CopyOptions instance with the same values.</returns>
        public CopyOptions Clone()
        {
            return new CopyOptions
            {
                SourcePath = SourcePath,
                DestinationPath = DestinationPath,
                Mode = Mode,
                ThreadCount = ThreadCount,
                RetryCount = RetryCount,
                RetryWaitTime = RetryWaitTime,
                CopySubdirectories = CopySubdirectories,
                IncludeEmptyDirectories = IncludeEmptyDirectories,
                CopyAttributes = CopyAttributes,
                CopyTimestamps = CopyTimestamps,
                RestartableMode = RestartableMode,
                BackupMode = BackupMode,
                VerboseOutput = VerboseOutput,
                IncludePatterns = new List<string>(IncludePatterns),
                ExcludePatterns = new List<string>(ExcludePatterns),
                ExcludeDirectories = new List<string>(ExcludeDirectories),
                CustomParameters = CustomParameters
            };
        }
    }

    /// <summary>
    /// Defines the available copy operation modes.
    /// </summary>
    public enum CopyMode
    {
        /// <summary>
        /// Mirror mode - Creates an exact copy, deleting files in destination that don't exist in source.
        /// </summary>
        Mirror,

        /// <summary>
        /// Backup mode - Incremental backup, copies only new and changed files.
        /// </summary>
        Backup,

        /// <summary>
        /// Sync mode - Synchronizes source and destination.
        /// </summary>
        Sync,

        /// <summary>
        /// Move mode - Moves files (deletes from source after copy).
        /// </summary>
        Move
    }
}
