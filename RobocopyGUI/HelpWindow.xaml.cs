using System.Windows;
using System.Windows.Controls;

using Button = System.Windows.Controls.Button;

namespace RobocopyGUI
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the HelpWindow class.
        /// </summary>
        public HelpWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Opens the help window to a specific section.
        /// </summary>
        /// <param name="section">The section to navigate to.</param>
        public void NavigateToSection(string section)
        {
            HideAllSections();

            switch (section)
            {
                case "QuickStart":
                    QuickStartSection.Visibility = Visibility.Visible;
                    break;
                case "FolderSelection":
                    FolderSelectionSection.Visibility = Visibility.Visible;
                    break;
                case "CopyModes":
                    CopyModesSection.Visibility = Visibility.Visible;
                    break;
                case "Performance":
                    PerformanceSection.Visibility = Visibility.Visible;
                    break;
                case "AdvancedOptions":
                    AdvancedOptionsSection.Visibility = Visibility.Visible;
                    break;
                case "Profiles":
                    ProfilesSection.Visibility = Visibility.Visible;
                    break;
                case "Shortcuts":
                    ShortcutsSection.Visibility = Visibility.Visible;
                    break;
                case "Troubleshooting":
                    TroubleshootingSection.Visibility = Visibility.Visible;
                    break;
                case "AboutRobocopy":
                    AboutRobocopySection.Visibility = Visibility.Visible;
                    break;
                default:
                    QuickStartSection.Visibility = Visibility.Visible;
                    break;
            }

            ContentScrollViewer.ScrollToTop();
        }

        /// <summary>
        /// Hides all content sections.
        /// </summary>
        private void HideAllSections()
        {
            QuickStartSection.Visibility = Visibility.Collapsed;
            FolderSelectionSection.Visibility = Visibility.Collapsed;
            CopyModesSection.Visibility = Visibility.Collapsed;
            PerformanceSection.Visibility = Visibility.Collapsed;
            AdvancedOptionsSection.Visibility = Visibility.Collapsed;
            ProfilesSection.Visibility = Visibility.Collapsed;
            ShortcutsSection.Visibility = Visibility.Collapsed;
            TroubleshootingSection.Visibility = Visibility.Collapsed;
            AboutRobocopySection.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Handles navigation button clicks.
        /// </summary>
        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string section)
            {
                NavigateToSection(section);
            }
        }

        /// <summary>
        /// Handles the Close button click.
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
