using System.Reflection;
using System.Windows;

namespace RobocopyGUI
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the AboutWindow class.
        /// </summary>
        public AboutWindow()
        {
            InitializeComponent();
            
            // Set version from assembly
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            if (version != null)
            {
                VersionText.Text = $"Version {version.Major}.{version.Minor}.{version.Build}";
            }
        }

        /// <summary>
        /// Handles the OK button click.
        /// </summary>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
