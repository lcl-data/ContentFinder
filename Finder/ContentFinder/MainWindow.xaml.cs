using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ContentFinder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        public MainWindow()
        {
            InitializeComponent();
            EnsureSettingsLoaded();
        }

        private void EnsureSettingsLoaded()
        {
            if (!Settings.FileExists)
            {
                Settings.Instance.ReadFromFile();
                SettingsDialog dialog = new SettingsDialog();
                dialog.DataContext = Settings.Instance;
                dialog.comboBoxShowWarning.SelectedIndex = Settings.Instance.ShowWarning.Equals(true) ? 0 : 1;
                dialog.Title = "Please make sure these settings are correct";
                dialog.ShowDialog();
                if (!dialog.DialogResult == true)
                {
                    App.Current.Shutdown();
                }
                Settings.Instance.SaveToFile();
            }
            else
            {
                Settings.Instance.ReadFromFile();
            }
        }

        private Regex content;
        private CancellationTokenSource tokenSource = new CancellationTokenSource();

        private void search_Click(object sender, RoutedEventArgs e)
        {
            if (btSearch.Content.ToString().ToLowerInvariant().Equals("search"))
            {
                this.btSearch.Content = "Stop?";
                this.label1.Content = "";
                EnableTimer();
                Log.Clear();
                lbResult.DataContext = null;
                lbResult.DisplayMemberPath = "";
                tokenSource = new CancellationTokenSource();

                string[] xmlFileDir = Settings.Instance.SearchFolder.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                content = new Regex(textBox1.Text.Trim());

                ProcessEveryFolder(xmlFileDir);
            }
            else
            {
                tokenSource.Cancel();
                try
                {
                    Task.WaitAll();
                }
                catch (AggregateException ex)
                {
                    foreach (var v in ex.InnerExceptions)
                        Log.Failure("msg: " + v.Message);
                }
                dispatcherTimer.IsEnabled = false;
                this.pbProgress.Value = this.pbProgress.Maximum;
                label1.Content = "Canceled";
                this.btSearch.Content = "Search";
            }

        }

        private void ProcessEveryFolder(string[] args)
        {
            var watch = Stopwatch.StartNew();
            Task<List<ContentUsage>>[] tasks = new Task<List<ContentUsage>>[args.Length];
            var ui = TaskScheduler.FromCurrentSynchronizationContext();
            List<ContentUsage> result = new List<ContentUsage>();
            for (int i = 0; i < args.Length; i++)
            {
                string tmp = args[i];
                int j = i;

                tasks[j] = Task.Factory.StartNew<List<ContentUsage>>(() =>
                {
                    return GetContainsMyContent(tmp);
                }, tokenSource.Token);

                tasks[j].ContinueWith(resultTask =>
                    result.AddRange(tasks[j].Result), CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, ui);

            }
            Task.Factory.ContinueWhenAll<List<ContentUsage>>(tasks,
                completedTasks =>
                {
                    label1.Content = string.Format("{0} occurences ", result.Count());
                    lbResult.ItemsSource = result;
                    lbResult.DisplayMemberPath = "usage";
                    dispatcherTimer.IsEnabled = false;
                    this.pbProgress.Value = this.pbProgress.Maximum;
                    this.btSearch.Content = "Search";
                }, CancellationToken.None, TaskContinuationOptions.None, ui);

        }

        private List<ContentUsage> GetContainsMyContent(string arg)
        {
            List<ContentUsage> allFileList = new List<ContentUsage>();
            FileSystemInfo[] allFile;
            try
            {
                allFile = (new DirectoryInfo(arg)).GetFileSystemInfos(Settings.Instance.FileFilter, SearchOption.AllDirectories);
            }
            catch (Exception ex)
            {
                Log.Failure(ex.Message);
                return new List<ContentUsage>();
            }
            List<ContentUsage> returnValue = new List<ContentUsage>(); ;
            foreach (var item in allFile)
            {
                tokenSource.Token.ThrowIfCancellationRequested();

                string fileType = "ContentFinder.SupportSuffix" + item.Extension.ToUpperInvariant() + "Class";
                Type type = Type.GetType(fileType);
                if (type != null)
                {
                    object SuffixClass = Activator.CreateInstance(type);
                    MethodInfo GetAllContent = type.GetMethod("GetAllContent");
                    BindingFlags flag = BindingFlags.Public | BindingFlags.Instance;
                    object[] parameters = new object[] { item.FullName, content.ToString() };
                    returnValue = (List<ContentUsage>)GetAllContent.Invoke(SuffixClass, flag, Type.DefaultBinder, parameters, null);
                    Console.WriteLine(returnValue);
                }
                else
                {
                    Log.Failure(string.Format("Not Support Sufffix : {0}", item.Extension.TrimStart('.')));
                }
                allFileList = allFileList.Union(returnValue).ToList();
            }
            return allFileList;

        }

        private void Option_Click(object sender, RoutedEventArgs e)
        {
            SettingsDialog dialog = new SettingsDialog();
            Settings.Instance.ReadFromFile();
            dialog.DataContext = Settings.Instance;
            dialog.comboBoxShowWarning.SelectedIndex = Settings.Instance.ShowWarning.Equals(true) ? 0 : 1;
            dialog.ShowDialog();
            if (dialog.DialogResult == true)
            {
                Settings.Instance.SaveToFile();
                lbResult.ItemsSource = null;
            }
        }

        private void lbResult_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ContentUsage contentUsage = (ContentUsage)((sender as System.Windows.Controls.ListBox).SelectedItem);
            string fileName = contentUsage.position.Filename;
            string fileType = "ContentFinder.SupportSuffix" + contentUsage.position.FileSuffix + "Class";
            Type type = Type.GetType(fileType);
            if (type != null)
            {
                object SuffixClass = Activator.CreateInstance(type);
                MethodInfo OpenFile = type.GetMethod("OpenFile");
                BindingFlags flag = BindingFlags.Public | BindingFlags.Instance;
                object[] parameters = new object[] { fileName };
                OpenFile.Invoke(SuffixClass, flag, Type.DefaultBinder, parameters, null);
            }
        }

        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        private void EnableTimer()
        {
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
            this.pbProgress.Value = 0;
        }

        void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (this.pbProgress.Value + 30 >= this.pbProgress.Maximum)
            {
                this.pbProgress.Value = 30;
            }
            else
            {
                this.pbProgress.Value++;
            }
        }

        private void textBox1_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                search_Click(sender, new RoutedEventArgs());
            }
        }



        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    tokenSource.Dispose();
                }
                disposed = true;
            }
        }

        ~MainWindow()
        {
            Dispose(false);
        }
    }
}
