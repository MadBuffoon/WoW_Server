using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
using System.Windows.Navigation;

namespace WoW_Server
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer statusCheckTimer;

        public MainWindow()
        {
            InitializeComponent();
            SaveLoadManager.LoadSettings();
            this.Width = SaveLoadManager.WindowWidth;
            this.Height = SaveLoadManager.WindowHeight;
            InitializeStatusCheckTimer();
            LoadUIValues();
        }

        private void InitializeStatusCheckTimer()
        {
            statusCheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            statusCheckTimer.Tick += StatusCheckTimer_Tick;
            statusCheckTimer.Start();
        }

        private void StatusCheckTimer_Tick(object sender, EventArgs e)
        {
            CheckStatuses();
        }

        private void CheckStatuses()
        {
            CheckMysqlStatus();
            CheckAuthStatus();
            CheckWorldStatus();
        }

        private async void CheckMysqlStatus()
        {
            bool isRunning = ProcessChecker.IsProcessRunning(SaveLoadManager.MysqlExeName);
            MysqlStatus.Text = isRunning ? "Status: Running" : "Status: Stopped";
            MysqlUptime.Text = isRunning ? $"Uptime: {ProcessChecker.GetProcessUptime(SaveLoadManager.MysqlExeName)}" : "Uptime: N/A";
            MysqlButton.Content = isRunning ? "Stop" : "Start";
            MysqlStatusImage.Source = LoadEmbeddedImage(isRunning ? "WoW_Server.Resources.Status-ColorBlock-Green.png" : "WoW_Server.Resources.Status-ColorBlock-Red.png");

            if (!isRunning && SaveLoadManager.MysqlAutoRestart)
            {
                SaveLoadManager.ZipLogFiles();
                await Task.Delay(500);
                ProcessStarter.StartProcess(SaveLoadManager.MysqlExePath);
                AppendLog($"AutoRestarted MySQL at {DateTime.Now:HH:mm:ss dd/MM/yyyy}");
            }
        }

        private async void CheckAuthStatus()
        {
            bool isRunning = ProcessChecker.IsProcessRunning(SaveLoadManager.AuthExeName);
            AuthStatus.Text = isRunning ? "Status: Running" : "Status: Stopped";
            AuthUptime.Text = isRunning ? $"Uptime: {ProcessChecker.GetProcessUptime(SaveLoadManager.AuthExeName)}" : "Uptime: N/A";
            AuthButton.Content = isRunning ? "Stop" : "Start";
            AuthStatusImage.Source = LoadEmbeddedImage(isRunning ? "WoW_Server.Resources.Status-ColorBlock-Green.png" : "WoW_Server.Resources.Status-ColorBlock-Red.png");

            if (!isRunning && SaveLoadManager.AuthAutoRestart)
            {
                SaveLoadManager.ZipLogFiles();
                await Task.Delay(500);
                ProcessStarter.StartProcess(SaveLoadManager.AuthExePath);
                AppendLog($"AutoRestarted Auth at {DateTime.Now:HH:mm:ss dd/MM/yyyy}");
            }
        }

        private async void CheckWorldStatus()
        {
            bool isRunning = ProcessChecker.IsProcessRunning(SaveLoadManager.WorldExeName);
            WorldStatus.Text = isRunning ? "Status: Running" : "Status: Stopped";
            WorldUptime.Text = isRunning ? $"Uptime: {ProcessChecker.GetProcessUptime(SaveLoadManager.WorldExeName)}" : "Uptime: N/A";
            WorldButton.Content = isRunning ? "Stop" : "Start";
            WorldStatusImage.Source = LoadEmbeddedImage(isRunning ? "WoW_Server.Resources.Status-ColorBlock-Green.png" : "WoW_Server.Resources.Status-ColorBlock-Red.png");

            if (!isRunning && SaveLoadManager.WorldAutoRestart)
            {
                SaveLoadManager.ZipLogFiles();
                await Task.Delay(500);
                ProcessStarter.StartProcess(SaveLoadManager.WorldExePath);
                AppendLog($"AutoRestarted World at {DateTime.Now:HH:mm:ss dd/MM/yyyy}");
            }
        }

        private async void MysqlButton_Click(object sender, RoutedEventArgs e)
        {
            if ((string)MysqlButton.Content == "Start")
            {
                ProcessStarter.StartProcess(SaveLoadManager.MysqlExePath, "--console");
                AppendLog($"Started MySQL at {DateTime.Now:HH:mm:ss dd/MM/yyyy}");
            }
            else
            {
                int.TryParse(MysqlKillWaitTime.Text, out int killWaitTime);
                await ProcessKiller.KillProcessGracefully(SaveLoadManager.MysqlExeName, killWaitTime);
                SaveLoadManager.MysqlAutoRestart = false;
                MysqlAutoRestart.IsChecked = false;
                SaveLoadManager.SaveSettings();
                AppendLog($"Stopped MySQL at {DateTime.Now:HH:mm:ss dd/MM/yyyy}");
            }
            CheckMysqlStatus();
        }

        private async void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            if ((string)AuthButton.Content == "Start")
            {
                ProcessStarter.StartProcess(SaveLoadManager.AuthExePath);
                AppendLog($"Started Auth at {DateTime.Now:HH:mm:ss dd/MM/yyyy}");
            }
            else
            {
                int.TryParse(AuthKillWaitTime.Text, out int killWaitTime);
                await ProcessKiller.KillProcessGracefully(SaveLoadManager.AuthExeName, killWaitTime);
                SaveLoadManager.AuthAutoRestart = false;
                AuthAutoRestart.IsChecked = false;
                SaveLoadManager.SaveSettings();
                AppendLog($"Stopped Auth at {DateTime.Now:HH:mm:ss dd/MM/yyyy}");
            }
            CheckAuthStatus();
        }

        private async void WorldButton_Click(object sender, RoutedEventArgs e)
        {
            if ((string)WorldButton.Content == "Start")
            {
                ProcessStarter.StartProcess(SaveLoadManager.WorldExePath);
                AppendLog($"Started World at {DateTime.Now:HH:mm:ss dd/MM/yyyy}");
            }
            else
            {
                int.TryParse(WorldKillWaitTime.Text, out int killWaitTime);
                string[] commands = { SaveLoadManager.WorldCommand1, SaveLoadManager.WorldCommand2, SaveLoadManager.WorldCommand3 };
                await ProcessKiller.KillProcessGracefully(SaveLoadManager.WorldExeName, killWaitTime, commands);
                SaveLoadManager.WorldAutoRestart = false;
                WorldAutoRestart.IsChecked = false;
                SaveLoadManager.SaveSettings();
                AppendLog($"Stopped World at {DateTime.Now:HH:mm:ss dd/MM/yyyy}");
            }
            CheckWorldStatus();
        }

        private void SetMysql_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                Title = "Select MySQL Executable"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SaveLoadManager.MysqlExeName = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                SaveLoadManager.MysqlExePath = openFileDialog.FileName;
                MysqlExeName.Text = SaveLoadManager.MysqlExeName;
                MysqlExePath.Text = SaveLoadManager.MysqlExePath;
                SaveLoadManager.SaveSettings();
            }
        }

        private void SetAuth_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                Title = "Select Auth Executable"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SaveLoadManager.AuthExeName = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                SaveLoadManager.AuthExePath = openFileDialog.FileName;
                AuthExeName.Text = SaveLoadManager.AuthExeName;
                AuthExePath.Text = SaveLoadManager.AuthExePath;
                SaveLoadManager.SaveSettings();
            }
        }

        private void SetWorld_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                Title = "Select World Executable"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SaveLoadManager.WorldExeName = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                SaveLoadManager.WorldExePath = openFileDialog.FileName;
                WorldExeName.Text = SaveLoadManager.WorldExeName;
                WorldExePath.Text = SaveLoadManager.WorldExePath;
                SaveLoadManager.SaveSettings();
            }
        }

        private void ClearMysql_Click(object sender, RoutedEventArgs e)
        {
            SaveLoadManager.MysqlExeName = null;
            SaveLoadManager.MysqlExePath = null;
            MysqlExeName.Text = string.Empty;
            MysqlExePath.Text = string.Empty;
            SaveLoadManager.SaveSettings();
        }

        private void ClearAuth_Click(object sender, RoutedEventArgs e)
        {
            SaveLoadManager.AuthExeName = null;
            SaveLoadManager.AuthExePath = null;
            AuthExeName.Text = string.Empty;
            AuthExePath.Text = string.Empty;
            SaveLoadManager.SaveSettings();
        }

        private void ClearWorld_Click(object sender, RoutedEventArgs e)
        {
            SaveLoadManager.WorldExeName = null;
            SaveLoadManager.WorldExePath = null;
            WorldExeName.Text = string.Empty;
            WorldExePath.Text = string.Empty;
            SaveLoadManager.SaveSettings();
        }

        private BitmapImage LoadEmbeddedImage(string resourcePath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourcePath))
            {
                if (stream == null)
                {
                    throw new Exception($"Unable to find resource: {resourcePath}");
                }

                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = stream;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                return image;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveLoadManager.WindowWidth = this.Width;
            SaveLoadManager.WindowHeight = this.Height;
            SaveLoadManager.SaveSettings();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SaveLoadManager.WindowWidth = this.Width;
            SaveLoadManager.WindowHeight = this.Height;
        }

        private void LoadUIValues()
        {
            MysqlExeName.Text = SaveLoadManager.MysqlExeName;
            MysqlExePath.Text = SaveLoadManager.MysqlExePath;
            MysqlKillWaitTime.Text = SaveLoadManager.MysqlKillWaitTime.ToString();
            MysqlAutoRestart.IsChecked = SaveLoadManager.MysqlAutoRestart;

            AuthExeName.Text = SaveLoadManager.AuthExeName;
            AuthExePath.Text = SaveLoadManager.AuthExePath;
            AuthKillWaitTime.Text = SaveLoadManager.AuthKillWaitTime.ToString();
            AuthAutoRestart.IsChecked = SaveLoadManager.AuthAutoRestart;

            WorldExeName.Text = SaveLoadManager.WorldExeName;
            WorldExePath.Text = SaveLoadManager.WorldExePath;
            WorldKillWaitTime.Text = SaveLoadManager.WorldKillWaitTime.ToString();
            WorldAutoRestart.IsChecked = SaveLoadManager.WorldAutoRestart;
            WorldCommand1.Text = SaveLoadManager.WorldCommand1;
            WorldCommand2.Text = SaveLoadManager.WorldCommand2;
            WorldCommand3.Text = SaveLoadManager.WorldCommand3;

            UpdateLogFilesPanel();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveLoadManager.MysqlExeName = MysqlExeName.Text;
            SaveLoadManager.MysqlExePath = MysqlExePath.Text;
            int.TryParse(MysqlKillWaitTime.Text, out int mysqlKillWaitTime);
            SaveLoadManager.MysqlKillWaitTime = mysqlKillWaitTime;
            SaveLoadManager.MysqlAutoRestart = MysqlAutoRestart.IsChecked.GetValueOrDefault();

            SaveLoadManager.AuthExeName = AuthExeName.Text;
            SaveLoadManager.AuthExePath = AuthExePath.Text;
            int.TryParse(AuthKillWaitTime.Text, out int authKillWaitTime);
            SaveLoadManager.AuthKillWaitTime = authKillWaitTime;
            SaveLoadManager.AuthAutoRestart = AuthAutoRestart.IsChecked.GetValueOrDefault();

            SaveLoadManager.WorldExeName = WorldExeName.Text;
            SaveLoadManager.WorldExePath = WorldExePath.Text;
            int.TryParse(WorldKillWaitTime.Text, out int worldKillWaitTime);
            SaveLoadManager.WorldKillWaitTime = worldKillWaitTime;
            SaveLoadManager.WorldAutoRestart = WorldAutoRestart.IsChecked.GetValueOrDefault();

            SaveLoadManager.WorldCommand1 = WorldCommand1.Text;
            SaveLoadManager.WorldCommand2 = WorldCommand2.Text;
            SaveLoadManager.WorldCommand3 = WorldCommand3.Text;

            SaveLoadManager.SaveSettings();
        }

        private void AutoRestart_Checked(object sender, RoutedEventArgs e)
        {
            SaveLoadManager.MysqlAutoRestart = MysqlAutoRestart.IsChecked.GetValueOrDefault();
            SaveLoadManager.AuthAutoRestart = AuthAutoRestart.IsChecked.GetValueOrDefault();
            SaveLoadManager.WorldAutoRestart = WorldAutoRestart.IsChecked.GetValueOrDefault();
            SaveLoadManager.SaveSettings();
        }

        private void AutoRestart_Unchecked(object sender, RoutedEventArgs e)
        {
            SaveLoadManager.MysqlAutoRestart = MysqlAutoRestart.IsChecked.GetValueOrDefault();
            SaveLoadManager.AuthAutoRestart = AuthAutoRestart.IsChecked.GetValueOrDefault();
            SaveLoadManager.WorldAutoRestart = WorldAutoRestart.IsChecked.GetValueOrDefault();
            SaveLoadManager.SaveSettings();
        }

        private void BrowseLogFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Log files (*.log)|*.log|All files (*.*)|*.*",
                Title = "Select Log File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SaveLoadManager.AddLogFile(openFileDialog.FileName);
                UpdateLogFilesPanel();
            }
        }

        private void RemoveLogFile_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var logFile = button.Tag as string;
            SaveLoadManager.RemoveLogFile(logFile);
            UpdateLogFilesPanel();
        }

        private void UpdateLogFilesPanel()
        {
            LogFilesPanel.Children.Clear();

            foreach (var logFile in SaveLoadManager.LogFiles)
            {
                var stackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 5, 0, 5)
                };

                var textBlock = new TextBlock
                {
                    Text = System.IO.Path.GetFileName(logFile),
                    Margin = new Thickness(0, 0, 10, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                var removeButton = new Button
                {
                    Content = "Remove",
                    Width = 75,
                    Tag = logFile
                };
                removeButton.Click += RemoveLogFile_Click;

                stackPanel.Children.Add(textBlock);
                stackPanel.Children.Add(removeButton);
                LogFilesPanel.Children.Add(stackPanel);
            }

            var browseButton = new Button
            {
                Content = "Browse",
                Width = 100,
                Margin = new Thickness(0, 5, 0, 5)
            };
            browseButton.Click += BrowseLogFile_Click;
            LogFilesPanel.Children.Add(browseButton);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void AppendLog(string message)
        {
            LogsTextBox.AppendText(message + Environment.NewLine);
            if (AutoScrollCheckBox.IsChecked == true)
            {
                LogsTextBox.ScrollToEnd();
            }
        }
    }
}
