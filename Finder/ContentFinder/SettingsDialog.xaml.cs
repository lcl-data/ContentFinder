using System.Windows;

namespace ContentFinder
{
    /// <summary>
    /// Interaction logic for SettingsDialog.xaml
    /// </summary>
    public partial class SettingsDialog : Window
    {
        public SettingsDialog()
        {
            InitializeComponent();         
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            Settings.Instance.SaveToFile();
            bool SettingsDialogValid = Settings.Instance.Validate();
            if (!SettingsDialogValid)
            {
                MessageBox.Show("Some of the SettingsDialog are invalid");
                e.Handled = false;
            }
            else
            {
                Settings.Instance.ReadFromFile();
                this.DialogResult = SettingsDialogValid;
                this.Close();
            }
           
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
