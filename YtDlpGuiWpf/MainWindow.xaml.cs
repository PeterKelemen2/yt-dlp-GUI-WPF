using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Interop;
using Microsoft.WindowsAPICodePack.Dialogs;
using Renci.SshNet;
using Path = System.IO.Path;

namespace YtDlpGuiWpf;

public partial class MainWindow : Window
{
    private YtdlpSettings _settings = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _settings;
    }

    private void PasteUrl_Click(object sender, RoutedEventArgs e)
    {
        if (Clipboard.ContainsText()) UrlTextBox.Text = Clipboard.GetText();
    }

    private async void DownloadButton_Click(object sender, RoutedEventArgs e)
    {
        string url = UrlTextBox.Text;
        string savePath = _settings.LocalSavePath;
        string ytDlpPath = _settings.YtDlpPath;
        string ytDlpArgs = _settings.YtDlpArguments;
        bool postInstall = _settings.EnablePostInstall;
        bool remoteTransfer = _settings.RunRemoteTransfer;
        string remoteHost = _settings.RemoteHost;
        string remoteUser = _settings.RemoteUsername;
        string remotePass = RemotePasswordBox.Password;
        string remoteLoc = _settings.RemoteLocation;

        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(savePath) ||
            string.IsNullOrWhiteSpace(ytDlpPath))
        {
            MessageBox.Show("Please fill in URL, Save Path, and yt-dlp Executable Path.");
            return;
        }

        // Extract output template from ytDlpArgs or use default
        string outputTemplate = "%(track, title, id)s.mp3";
        var match = System.Text.RegularExpressions.Regex.Match(ytDlpArgs, @"-o\s+""([^""]+)""");
        if (match.Success)
        {
            outputTemplate = match.Groups[1].Value;
        }

        try
        {
            // Clear output box
            OutputTextBox.Clear();

            // 1) Get expected filename
            string filename = await GetYtDlpFilenameAsync(ytDlpPath, url, outputTemplate);
            filename = Path.ChangeExtension(filename, ".mp3");
            if (string.IsNullOrWhiteSpace(filename))
            {
                MessageBox.Show("Could not determine output filename.");
                return;
            }

            string fullFilePath = Path.Combine(savePath, filename);

            AppendOutput($"Filename resolved: {filename}");

            // 2) Run actual download
            var downloadArgs = $"{ytDlpArgs} -o \"{Path.Combine(savePath, filename)}\" \"{url}\"";
            await RunProcessAsync(ytDlpPath, downloadArgs);

            AppendOutput("Download complete.");

            // 3) Optionally upload file
            if (postInstall && remoteTransfer)
            {
                AppendOutput("Uploading to remote...");
                bool success =
                    await UploadFileAsync(fullFilePath, "petiserver.home", remoteUser, remotePass, remoteLoc);
                AppendOutput(success ? "Upload succeeded." : "Upload failed.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}");
        }
    }

// Helper method to run a process asynchronously and capture output
    private async Task RunProcessAsync(string exePath, string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var process = new Process { StartInfo = psi };

        process.OutputDataReceived += (s, e) =>
        {
            if (e.Data != null) AppendOutput(e.Data);
        };
        process.ErrorDataReceived += (s, e) =>
        {
            if (e.Data != null) AppendOutput($"ERR: {e.Data}");
        };

        process.Start();

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();
    }

    // Helper method to append text to OutputTextBox on UI thread
    private void AppendOutput(string text)
    {
        Dispatcher.Invoke(() =>
        {
            OutputTextBox.AppendText(text + Environment.NewLine);
            OutputTextBox.ScrollToEnd();
        });
    }

    // Method to get filename from yt-dlp
    private async Task<string> GetYtDlpFilenameAsync(string ytDlpPath, string url, string outputTemplate)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = ytDlpPath,
            Arguments = $"--get-filename -o \"{outputTemplate}\" \"{url}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
        };

        using var process = new Process { StartInfo = startInfo };

        process.Start();

        string filename = await process.StandardOutput.ReadLineAsync();

        await process.WaitForExitAsync();

        return filename;
    }

    private void BrowseLocalPath_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new CommonOpenFileDialog { IsFolderPicker = true };
        if (dialog.ShowDialog() == CommonFileDialogResult.Ok) LocalPathTextBox.Text = dialog.FileName;
    }

    private void BrowseYTDLPPath_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "yt-dlp Executable|yt-dlp.exe;yt-dlp",
            Title = "Select yt-dlp Executable"
        };

        if (dialog.ShowDialog() == true) YtDlpPathTextBox.Text = dialog.FileName;
    }


    public async Task<bool> UploadFileAsync(string localFilePath, string remoteHost, string remoteUser,
        string remotePass, string remotePath)
    {
        Console.WriteLine($"{localFilePath} | {remoteHost} | {remoteUser} | {remotePass} | {remotePath}");
        try
        {
            await Task.Run(() =>
            {
                using var sftp = new SftpClient(remoteHost, remoteUser, remotePass);
                sftp.Connect();

                if (!sftp.IsConnected)
                    throw new Exception("SFTP connection failed.");

                string remoteFilePath = remotePath;

                if (!remotePath.EndsWith("/") && !remotePath.EndsWith("\\")) remoteFilePath = remotePath + "/";

                remoteFilePath += Path.GetFileName(localFilePath);

                Console.WriteLine($"Uploading to remote file path: {remoteFilePath}");

                using var fileStream = File.OpenRead(localFilePath);
                sftp.UploadFile(fileStream, remoteFilePath);

                sftp.Disconnect();
            });

            return true; // success
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Upload failed: {ex.Message}", "SFTP Upload Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false; // failed
        }
    }

    private enum DWMWINDOWATTRIBUTE : uint
    {
        DWMWA_USE_IMMERSIVE_DARK_MODE = 20
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attr, ref bool attrValue,
        int attrSize);

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        var attribute = DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE;
        var useDarkMode = true;
        DwmSetWindowAttribute(hwnd, attribute, ref useDarkMode, Marshal.SizeOf(typeof(bool)));

        if (File.Exists(YtdlpSettings.ConfigPath))
        {
            try
            {
                var json = File.ReadAllText(YtdlpSettings.ConfigPath);
                var loaded = JsonSerializer.Deserialize<YtdlpSettings>(json);
                if (loaded != null)
                    _settings = loaded;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}");
            }
        }

        DataContext = _settings;
        RemotePasswordBox.Password = _settings.RemotePassword;
    }

    private void MainWindow_Closing(object sender, CancelEventArgs e)
    {
        _settings.RemotePassword = RemotePasswordBox.Password;
        var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(YtdlpSettings.ConfigPath, json);
    }

    private void RemotePasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        _settings.RemotePassword = RemotePasswordBox.Password;
    }
}