using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace YtDlpGuiWpf;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void PasteUrl_Click(object sender, RoutedEventArgs e)
    {
        if (Clipboard.ContainsText()) UrlTextBox.Text = Clipboard.GetText();
    }

    private async void DownloadButton_Click(object sender, RoutedEventArgs e)
    {
        string url = UrlTextBox.Text.Trim();
        string savePath = LocalPathTextBox.Text.Trim();
        string ytDlpPath = YtDlpPathTextBox.Text.Trim();
        string ytDlpArgs = YtDlpArgsTextBox.Text.Trim();
        bool postInstall = EnablePostInstallCheckBox.IsChecked == true;
        string remoteUser = RemoteUsernameTextBox.Text;
        string remotePass = RemotePasswordBox.Password;
        string remoteLoc = RemoteLocationTextBox.Text;
        string postScript = PostTransferScriptPathTextBox.Text;

        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(ytDlpPath) ||
            string.IsNullOrWhiteSpace(savePath))
        {
            MessageBox.Show("Please fill in URL, yt-dlp path, and save path.", "Missing Information",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        OutputTextBox.Text = "";

        await Task.Run(() =>
        {
            var finalArgs = new StringBuilder();
            finalArgs.Append($"\"{url}\" ");
            finalArgs.Append($"-P \"{savePath}\" ");
            if (!string.IsNullOrWhiteSpace(ytDlpArgs))
                finalArgs.Append(ytDlpArgs);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ytDlpPath,
                    Arguments = finalArgs.ToString(),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            process.OutputDataReceived += (s, args) =>
            {
                if (args.Data != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        OutputTextBox.AppendText(args.Data + Environment.NewLine);
                        OutputTextBox.ScrollToEnd();
                    });
                }
            };

            process.ErrorDataReceived += (s, args) =>
            {
                if (args.Data != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        OutputTextBox.AppendText("[ERROR] " + args.Data + Environment.NewLine);
                        OutputTextBox.ScrollToEnd();
                    });
                }
            };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                Dispatcher.Invoke(() => { OutputTextBox.AppendText("Download finished." + Environment.NewLine); });

                if (postInstall && File.Exists(postScript))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = postScript,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    OutputTextBox.AppendText($"[EXCEPTION] {ex.Message}" + Environment.NewLine);
                });
            }
        });
    }


    private void BrowseLocalPath_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new CommonOpenFileDialog
        {
            IsFolderPicker = true
        };

        if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
        {
            LocalPathTextBox.Text = dialog.FileName;
        }
    }

    private void BrowseYTDLPPath_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "yt-dlp Executable|yt-dlp.exe;yt-dlp",
            Title = "Select yt-dlp Executable"
        };

        if (dialog.ShowDialog() == true)
        {
            YtDlpPathTextBox.Text = dialog.FileName;
        }
    }

    private void LoadSettings()
    {
        if (File.Exists(Common.ConfigPath))
        {
            try
            {
                var json = File.ReadAllText(Common.ConfigPath);
                var settings = JsonSerializer.Deserialize<YtdlpSettings>(json);
                if (settings != null)
                {
                    YtDlpPathTextBox.Text = settings.YtDlpPath;
                    LocalPathTextBox.Text = settings.LocalSavePath;
                    YtDlpArgsTextBox.Text = settings.YtDlpArguments;
                    EnablePostInstallCheckBox.IsChecked = settings.EnablePostInstall;
                    RemoteUsernameTextBox.Text = settings.RemoteUsername;
                    RemotePasswordBox.Password = settings.RemotePassword;
                    RemoteLocationTextBox.Text = settings.RemoteLocation;
                    PostTransferScriptPathTextBox.Text = settings.PostTransferScriptPath;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load settings. Using defaults.\n\n" + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }


    private void SaveSettings()
    {
        var settings = new YtdlpSettings
        {
            YtDlpPath = YtDlpPathTextBox.Text,
            LocalSavePath = LocalPathTextBox.Text,
            YtDlpArguments = YtDlpArgsTextBox.Text,
            EnablePostInstall = EnablePostInstallCheckBox.IsChecked ?? false,
            RemoteUsername = RemoteUsernameTextBox.Text,
            RemotePassword = RemotePasswordBox.Password,
            RemoteLocation = RemoteLocationTextBox.Text,
            PostTransferScriptPath = PostTransferScriptPathTextBox.Text
        };

        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Common.ConfigPath, json);
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        LoadSettings();
    }

    private void MainWindow_Closing(object sender, CancelEventArgs e)
    {
        SaveSettings();
    }
}