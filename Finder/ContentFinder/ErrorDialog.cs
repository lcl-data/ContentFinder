
namespace ContentFinder
{
    class ErrorDialog
    {
        public static void Show(string message)
        {
            System.Windows.MessageBox.Show(string.Format(message + "\n-----------Help info-----------\nFile Bugs(http://contentfinder.codeplex.com/workitem/list/basic)\n"), "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }
}
