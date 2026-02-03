using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using RobocopyGUI.Models;
using RobocopyGUI.Services;

namespace RobocopyGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly RobocopyService _robocopyService;
        private readonly LoggingService _loggingService;
        private bool _isRunning;

        // Cached brushes for status indicator to avoid creating new instances
        private static readonly SolidColorBrush ReadyBrush = new(Color.FromRgb(158, 158, 158));
        private static readonly SolidColorBrush RunningBrush = new(Color.FromRgb(33, 150, 243));
        private static readonly SolidColorBrush SuccessBrush = new(Color.FromRgb(76, 175, 80));
        private static readonly SolidColorBrush WarningBrush = new(Color.FromRgb(255, 193, 7));
        private static readonly SolidColorBrush ErrorBrush = new(Color.FromRgb(244, 67, 54));

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            
            _robocopyService = new RobocopyService();
            _loggingService = new LoggingService();

            // Subscribe to events
            _robocopyService.OutputReceived += RobocopyService_OutputReceived;
            _robocopyService.ErrorReceived += RobocopyService_ErrorReceived;
            _robocopyService.CopyCompleted += RobocopyService_CopyCompleted;

            // Initialize UI
            UpdateCommandPreview();
            UpdateStatus("Ready", StatusType.Ready);

            // Log startup
            _loggingService.Info("Robocopy GUI started");
            AppendLog("Welcome to Robocopy GUI!", LogType.Info);
            AppendLog("Select source and destination folders to begin.", LogType.Info);
        }

        #region Event Handlers

        /// <summary>
        /// Handles the Browse Source button click.
        /// </summary>
        private void BrowseSource_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Source Folder",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SourcePathTextBox.Text = dialog.SelectedPath;
                UpdateCommandPreview();
                _loggingService.Info($"Source path selected: {dialog.SelectedPath}");
            }
        }

        /// <summary>
        /// Handles the Browse Destination button click.
        /// </summary>
        private void BrowseDestination_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Destination Folder",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DestinationPathTextBox.Text = dialog.SelectedPath;
                UpdateCommandPreview();
                _loggingService.Info($"Destination path selected: {dialog.SelectedPath}");
            }
        }

        /// <summary>
        /// Handles drag and drop for source path.
        /// </summary>
        private void SourcePath_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (paths.Length > 0 && Directory.Exists(paths[0]))
                {
                    SourcePathTextBox.Text = paths[0];
                    UpdateCommandPreview();
                }
            }
        }

        /// <summary>
        /// Handles drag and drop for destination path.
        /// </summary>
        private void DestinationPath_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (paths.Length > 0 && Directory.Exists(paths[0]))
                {
                    DestinationPathTextBox.Text = paths[0];
                    UpdateCommandPreview();
                }
            }
        }

        /// <summary>
        /// Handles drag over for text boxes.
        /// </summary>
        private void TextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) 
                ? DragDropEffects.Copy 
                : DragDropEffects.None;
        }

        /// <summary>
        /// Handles copy mode selection change.
        /// </summary>
        private void CopyModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateModeWarning();
            UpdateCommandPreview();
        }

        /// <summary>
        /// Handles the Start button click.
        /// </summary>
        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate inputs
            var options = BuildCopyOptions();
            var errors = _robocopyService.ValidateOptions(options);

            if (errors.Count > 0)
            {
                MessageBox.Show(
                    string.Join("\n", errors),
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Confirm destructive operations
            if (options.Mode == CopyMode.Mirror || options.Mode == CopyMode.Move)
            {
                var confirmMessage = options.Mode == CopyMode.Mirror
                    ? "Mirror mode will DELETE files in the destination that don't exist in the source.\n\nAre you sure you want to continue?"
                    : "Move mode will DELETE files from the source after copying.\n\nAre you sure you want to continue?";

                if (MessageBox.Show(confirmMessage, "Confirm Operation", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            // Start operation
            _isRunning = true;
            UpdateUIForRunningState(true);
            UpdateStatus("Running...", StatusType.Running);
            AppendLog($"Starting copy operation: {options.Mode} mode", LogType.Info);
            AppendLog($"Source: {options.SourcePath}", LogType.Info);
            AppendLog($"Destination: {options.DestinationPath}", LogType.Info);
            AppendLog($"Threads: {options.ThreadCount}", LogType.Info);
            AppendLog("---", LogType.Info);

            _loggingService.Info($"Copy operation started: {_robocopyService.GetFullCommand(options)}");

            try
            {
                var result = await _robocopyService.ExecuteAsync(options);
                
                // Result will be handled in CopyCompleted event
            }
            catch (Exception ex)
            {
                AppendLog($"Error: {ex.Message}", LogType.Error);
                _loggingService.Error("Copy operation failed", ex);
                UpdateStatus("Error", StatusType.Error);
            }
            finally
            {
                _isRunning = false;
                UpdateUIForRunningState(false);
            }
        }

        /// <summary>
        /// Handles the Stop button click.
        /// </summary>
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(
                "Are you sure you want to stop the current operation?",
                "Confirm Stop",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _robocopyService.Cancel();
                AppendLog("Operation cancelled by user.", LogType.Warning);
                _loggingService.Warning("Copy operation cancelled by user");
            }
        }

        /// <summary>
        /// Handles the Copy Command button click.
        /// </summary>
        private void CopyCommand_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(CommandPreviewTextBox.Text))
            {
                Clipboard.SetText(CommandPreviewTextBox.Text);
                BottomStatusText.Text = "Command copied to clipboard!";
            }
        }

        /// <summary>
        /// Handles the Clear Log button click.
        /// </summary>
        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            LogTextBox.Clear();
        }

        /// <summary>
        /// Handles the Export Log button click.
        /// </summary>
        private void ExportLog_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Log Files (*.log)|*.log|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                DefaultExt = "log",
                FileName = $"RobocopyGUI_Log_{DateTime.Now:yyyyMMdd_HHmmss}.log"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(dialog.FileName, LogTextBox.Text);
                    MessageBox.Show("Log exported successfully!", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                    _loggingService.Info($"Log exported to: {dialog.FileName}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to export log: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Handles the Save Profile button click.
        /// </summary>
        private void SaveProfile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Profile Files (*.json)|*.json",
                DefaultExt = "json",
                InitialDirectory = CopyProfile.GetProfilesDirectory(),
                FileName = "MyProfile.json"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var profile = new CopyProfile
                    {
                        Name = Path.GetFileNameWithoutExtension(dialog.FileName),
                        Description = "Saved profile",
                        Options = BuildCopyOptions()
                    };

                    profile.Save(dialog.FileName);
                    MessageBox.Show("Profile saved successfully!", "Save Profile", MessageBoxButton.OK, MessageBoxImage.Information);
                    _loggingService.Info($"Profile saved: {dialog.FileName}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to save profile: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Handles the Load Profile button click.
        /// </summary>
        private void LoadProfile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Profile Files (*.json)|*.json",
                DefaultExt = "json",
                InitialDirectory = CopyProfile.GetProfilesDirectory()
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var profile = CopyProfile.Load(dialog.FileName);
                    ApplyProfile(profile);
                    MessageBox.Show($"Profile '{profile.Name}' loaded successfully!", "Load Profile", MessageBoxButton.OK, MessageBoxImage.Information);
                    _loggingService.Info($"Profile loaded: {dialog.FileName}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load profile: {ex.Message}", "Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Handles output received from the Robocopy process.
        /// </summary>
        private void RobocopyService_OutputReceived(object? sender, string e)
        {
            Dispatcher.Invoke(() =>
            {
                AppendLog(e, LogType.Output);
            });
        }

        /// <summary>
        /// Handles error received from the Robocopy process.
        /// </summary>
        private void RobocopyService_ErrorReceived(object? sender, string e)
        {
            Dispatcher.Invoke(() =>
            {
                AppendLog(e, LogType.Error);
            });
        }

        /// <summary>
        /// Handles copy operation completion.
        /// </summary>
        private void RobocopyService_CopyCompleted(object? sender, CopyResult e)
        {
            Dispatcher.Invoke(() =>
            {
                AppendLog("---", LogType.Info);
                AppendLog($"Operation completed: Exit code {e.ExitCode}", e.Success ? LogType.Success : LogType.Error);
                AppendLog($"Status: {e.ExitCodeDescription}", LogType.Info);
                AppendLog($"Duration: {e.Duration:hh\\:mm\\:ss}", LogType.Info);
                AppendLog($"Files: {e.FilesCopied} copied, {e.FilesSkipped} skipped, {e.FilesFailed} failed", LogType.Info);

                if (e.WasCancelled)
                {
                    UpdateStatus("Cancelled", StatusType.Warning);
                }
                else if (e.Success)
                {
                    UpdateStatus("Completed", StatusType.Success);
                    CopyProgressBar.Value = 100;
                }
                else
                {
                    UpdateStatus("Completed with errors", StatusType.Error);
                }

                BottomStatusText.Text = e.Summary;
                _loggingService.Info($"Copy completed: {e.Summary}");
            });
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Builds copy options from the UI controls.
        /// </summary>
        private CopyOptions BuildCopyOptions()
        {
            var mode = CopyMode.Backup;
            if (CopyModeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                mode = selectedItem.Tag?.ToString() switch
                {
                    "Mirror" => CopyMode.Mirror,
                    "Sync" => CopyMode.Sync,
                    "Move" => CopyMode.Move,
                    _ => CopyMode.Backup
                };
            }

            return new CopyOptions
            {
                SourcePath = SourcePathTextBox.Text,
                DestinationPath = DestinationPathTextBox.Text,
                Mode = mode,
                ThreadCount = int.TryParse(ThreadCountTextBox.Text, out var threads) ? Math.Clamp(threads, 1, 128) : 8,
                RetryCount = int.TryParse(RetryCountTextBox.Text, out var retries) ? Math.Max(retries, 0) : 3,
                RetryWaitTime = int.TryParse(RetryWaitTextBox.Text, out var wait) ? Math.Max(wait, 0) : 5,
                CopySubdirectories = CopySubdirectoriesCheckBox.IsChecked ?? true,
                IncludeEmptyDirectories = IncludeEmptyDirsCheckBox.IsChecked ?? true,
                CopyAttributes = CopyAttributesCheckBox.IsChecked ?? true,
                RestartableMode = RestartableModeCheckBox.IsChecked ?? false,
                VerboseOutput = VerboseOutputCheckBox.IsChecked ?? false,
                CustomParameters = CustomParametersTextBox.Text
            };
        }

        /// <summary>
        /// Applies a profile to the UI controls.
        /// </summary>
        private void ApplyProfile(CopyProfile profile)
        {
            var options = profile.Options;

            SourcePathTextBox.Text = options.SourcePath;
            DestinationPathTextBox.Text = options.DestinationPath;

            // Set copy mode
            var modeTag = options.Mode.ToString();
            foreach (ComboBoxItem item in CopyModeComboBox.Items)
            {
                if (item.Tag?.ToString() == modeTag)
                {
                    CopyModeComboBox.SelectedItem = item;
                    break;
                }
            }

            ThreadCountTextBox.Text = options.ThreadCount.ToString();
            RetryCountTextBox.Text = options.RetryCount.ToString();
            RetryWaitTextBox.Text = options.RetryWaitTime.ToString();

            CopySubdirectoriesCheckBox.IsChecked = options.CopySubdirectories;
            IncludeEmptyDirsCheckBox.IsChecked = options.IncludeEmptyDirectories;
            CopyAttributesCheckBox.IsChecked = options.CopyAttributes;
            RestartableModeCheckBox.IsChecked = options.RestartableMode;
            VerboseOutputCheckBox.IsChecked = options.VerboseOutput;
            CustomParametersTextBox.Text = options.CustomParameters;

            UpdateCommandPreview();
        }

        /// <summary>
        /// Updates the command preview text.
        /// </summary>
        private void UpdateCommandPreview()
        {
            var options = BuildCopyOptions();
            CommandPreviewTextBox.Text = _robocopyService.GetFullCommand(options);
        }

        /// <summary>
        /// Updates the mode warning message.
        /// </summary>
        private void UpdateModeWarning()
        {
            if (CopyModeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var mode = selectedItem.Tag?.ToString();

                switch (mode)
                {
                    case "Mirror":
                        ModeWarningText.Text = "Warning: Mirror mode will DELETE files in the destination that don't exist in the source!";
                        ModeWarningBorder.Visibility = Visibility.Visible;
                        break;
                    case "Move":
                        ModeWarningText.Text = "Warning: Move mode will DELETE files from the source after copying!";
                        ModeWarningBorder.Visibility = Visibility.Visible;
                        break;
                    default:
                        ModeWarningBorder.Visibility = Visibility.Collapsed;
                        break;
                }
            }
        }

        /// <summary>
        /// Updates the UI for running/stopped state.
        /// </summary>
        private void UpdateUIForRunningState(bool isRunning)
        {
            StartButton.IsEnabled = !isRunning;
            StopButton.IsEnabled = isRunning;
            SourcePathTextBox.IsEnabled = !isRunning;
            DestinationPathTextBox.IsEnabled = !isRunning;
            CopyModeComboBox.IsEnabled = !isRunning;
            ThreadCountTextBox.IsEnabled = !isRunning;
            RetryCountTextBox.IsEnabled = !isRunning;
            RetryWaitTextBox.IsEnabled = !isRunning;

            if (isRunning)
            {
                CopyProgressBar.IsIndeterminate = true;
            }
            else
            {
                CopyProgressBar.IsIndeterminate = false;
            }
        }

        /// <summary>
        /// Updates the status indicator and text.
        /// </summary>
        private void UpdateStatus(string text, StatusType status)
        {
            StatusText.Text = text;
            StatusIndicator.Fill = status switch
            {
                StatusType.Ready => ReadyBrush,
                StatusType.Running => RunningBrush,
                StatusType.Success => SuccessBrush,
                StatusType.Warning => WarningBrush,
                StatusType.Error => ErrorBrush,
                _ => ReadyBrush
            };
        }

        /// <summary>
        /// Appends a message to the log text box.
        /// </summary>
        private void AppendLog(string message, LogType type)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var prefix = type switch
            {
                LogType.Info => "[INFO]",
                LogType.Warning => "[WARN]",
                LogType.Error => "[ERROR]",
                LogType.Success => "[OK]",
                LogType.Output => "",
                _ => ""
            };

            var line = string.IsNullOrEmpty(prefix) 
                ? $"{message}\n" 
                : $"[{timestamp}] {prefix} {message}\n";

            LogTextBox.AppendText(line);
            LogScrollViewer.ScrollToEnd();
        }

        #endregion

        /// <summary>
        /// Called when the window is closing.
        /// </summary>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (_isRunning)
            {
                var result = MessageBox.Show(
                    "A copy operation is in progress. Are you sure you want to exit?",
                    "Confirm Exit",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }

                _robocopyService.Cancel();
            }

            _loggingService.Info("Robocopy GUI closed");
            base.OnClosing(e);
        }
    }

    /// <summary>
    /// Defines status types for the status indicator.
    /// </summary>
    public enum StatusType
    {
        Ready,
        Running,
        Success,
        Warning,
        Error
    }

    /// <summary>
    /// Defines log message types.
    /// </summary>
    public enum LogType
    {
        Info,
        Warning,
        Error,
        Success,
        Output
    }
}
