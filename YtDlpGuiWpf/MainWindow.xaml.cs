﻿using System.ComponentModel;
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
    public YtdlpSettings Settings { get; set; } = new();
    public LocalizationViewModel LocVM { get; } = new();
    bool isDownloading = false;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void PasteUrl_Click(object sender, RoutedEventArgs e)
    {
        if (Clipboard.ContainsText()) UrlTextBox.Text = Clipboard.GetText();
    }

    private async void DownloadButton_Click(object sender, RoutedEventArgs e)
    {
        string url = UrlTextBox.Text;
        string savePath = Settings.LocalSavePath;
        string ytDlpPath = Settings.YtDlpPath;
        string ytDlpArgs = Settings.YtDlpArguments;
        string ytDlpNaming = Settings.YtDlpNamingScheme;
        bool postInstall = Settings.EnablePostInstall;
        bool remoteTransfer = Settings.RunRemoteTransfer;
        bool remoteScript = Settings.RunRemoteScript;
        string remoteScriptPath = Settings.PostTransferScriptPath;
        string remoteHost = Settings.RemoteHost;
        string remoteUser = Settings.RemoteUsername;
        string remotePass = RemotePasswordBox.Password;
        string remoteLoc = Settings.RemoteLocation;

        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(savePath) ||
            string.IsNullOrWhiteSpace(ytDlpPath))
        {
            MessageBox.Show("Please fill in URL, Save Path, and yt-dlp Executable Path.");
            return;
        }

        try
        {
            isDownloading = true;
            DownloadBtn.IsEnabled = false;
            // Clear output box
            OutputTextBox.Clear();
            
            // 2) Run actual download
            var downloadArgs = $"{ytDlpArgs} --print after_move:filepath --verbose -o \"{Path.Combine(savePath, ytDlpNaming)}\" \"{url}\"";
            string stdout = await RunProcessAsync(ytDlpPath, downloadArgs);

            string fullFilePath = stdout
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .LastOrDefault() ?? "";
            
            Console.WriteLine("FullFilePath: " +  fullFilePath);
            AppendOutput("Download complete.");

            // 3) Optionally upload file
            if (postInstall)
            {
                if (remoteTransfer)
                {
                    AppendOutput("Uploading to remote...");
                    bool success =
                        await UploadFileAsync(fullFilePath, remoteHost, remoteUser, remotePass, remoteLoc);
                    AppendOutput(success ? "Upload succeeded." : "Upload failed.");
                }

                if (remoteScript)
                {
                    AppendOutput("Running remote script...");
                    string selectedOS = SelectedOS.SelectedItem as string ?? string.Empty;
                    bool isWindows = string.Equals(selectedOS, "Windows", StringComparison.OrdinalIgnoreCase);

                    bool success = await RunRemoteExecutableAsync(remoteHost, remoteUser, remotePass, remoteScriptPath,
                        isWindows);
                    AppendOutput(success ? "Script succeeded." : "Something failed.");
                }
            }
            
            isDownloading = false;
            DownloadBtn.IsEnabled = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}");
            isDownloading = false;
            DownloadBtn.IsEnabled = true;
        }
    }
    
    private async Task<bool> RunRemoteExecutableAsync(string host, string username, string password, string exePath,
        bool isWindows)
    {
        try
        {
            using var ssh = new SshClient(host, username, password);
            ssh.Connect();

            string command;
            if (isWindows)
            {
                AppendOutput("Windows machine found!");
                // Wrap path in quotes, run with cmd /c for safety
                command = $"cmd /c \"\"{exePath}\"\"";
            }
            else
            {
                AppendOutput("Linux machine found!");
                // Linux, run directly
                command = $"bash \"{exePath}\"";
                Console.WriteLine($"Command: {command}");
            }

            var result = ssh.RunCommand(command);
            ssh.Disconnect();

            AppendOutput($"Remote executable output: {result.Result}");
            if (!string.IsNullOrEmpty(result.Error))
                AppendOutput($"Remote executable error: {result.Error}");

            return string.IsNullOrEmpty(result.Error);
        }
        catch (Exception ex)
        {
            AppendOutput($"Exception while running remote exe: {ex.Message}");
            return false;
        }
    }


    // Helper method to run a process asynchronously and capture output, also return the output file name
    private async Task<string> RunProcessAsync(string exePath, string arguments)
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

        var stdoutBuilder = new StringBuilder();

        using var process = new Process { StartInfo = psi };

        process.OutputDataReceived += (s, e) =>
        {
            if (e.Data != null)
            {
                AppendOutput($"Output file: {e.Data}");
                Console.WriteLine(e.Data);
                stdoutBuilder.AppendLine(e.Data);  // Collect stdout (file path lines)
            }
        };

        process.ErrorDataReceived += (s, e) =>
        {
            if (e.Data != null)
            {
                Console.WriteLine(e.Data);
                if(!string.IsNullOrWhiteSpace(e.Data.Trim())) AppendOutput($"[yt-dlp] {e.Data}");
            }
        };

        AppendOutput("Starting process...");
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        // Return trimmed stdout (the file path lines)
        return stdoutBuilder.ToString().Trim();
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

        if (File.Exists(Common.ConfigPath))
        {
            try
            {
                var json = File.ReadAllText(Common.ConfigPath);
                var loaded = JsonSerializer.Deserialize<YtdlpSettings>(json);
                if (loaded != null)
                    Settings = loaded;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}");
            }
        }

        // DataContext = Settings;
        RemotePasswordBox.Password = Settings.RemotePassword;
        
        Loc.SetCulture(Settings.Culture);
    }

    private void MainWindow_Closing(object sender, CancelEventArgs e)
    {
        Settings.RemotePassword = RemotePasswordBox.Password;
        var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(Common.ConfigPath, json);
    }

    private void RemotePasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        Settings.RemotePassword = RemotePasswordBox.Password;
    }

    private void ToggleLanguage_Click(object sender, RoutedEventArgs e)
    {
        var current = Loc.GetCurrentCulture().Name;
        var newCulture = current.StartsWith("hu") ? "en" : "hu"; 
        
        Settings.Culture = newCulture;
        Loc.SetCulture(newCulture);

        // Tell the ViewModel to notify UI to update localized strings
        LocVM.RaiseAllPropertiesChanged();
    }
}