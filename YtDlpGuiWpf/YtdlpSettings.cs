using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;

namespace YtDlpGuiWpf;

public class YtdlpSettings : INotifyPropertyChanged
{
    public const string ConfigPath = "config.json";
    
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    
    private string _ytDlpPath = string.Empty;
    public string YtDlpPath
    {
        get => _ytDlpPath;
        set { _ytDlpPath = value; OnPropertyChanged(); }
    }
    
    private string _localSavePath = string.Empty;
    public string LocalSavePath
    {
        get => _localSavePath;
        set { _localSavePath = value; OnPropertyChanged(); }
    }

    private string _ytDlpArguments = "-x --audio-format mp3 --embed-thumbnail --add-metadata -o \"%(track, title, id)s.%(ext)s\"";
    public string YtDlpArguments
    {
        get => _ytDlpArguments;
        set { _ytDlpArguments = value; OnPropertyChanged(); }
    }
    
    private bool _enablePostInstall;
    public bool EnablePostInstall
    {
        get => _enablePostInstall;
        set { _enablePostInstall = value; OnPropertyChanged(); }
    }

    private string _remoteHost = string.Empty;
    public string RemoteHost
    {
        get => _remoteHost;
        set { _remoteHost = value; OnPropertyChanged(); }
    }

    private string _remoteUsername = string.Empty;
    public string RemoteUsername
    {
        get => _remoteUsername;
        set { _remoteUsername = value; OnPropertyChanged(); }
    }

    private string _remotePassword = string.Empty;
    public string RemotePassword
    {
        get => _remotePassword;
        set { _remotePassword = value; OnPropertyChanged(); }
    }

    private string _remoteLocation = string.Empty;
    public string RemoteLocation
    {
        get => _remoteLocation;
        set { _remoteLocation = value; OnPropertyChanged(); }
    }

    private string _postTransferScriptPath = string.Empty;
    public string PostTransferScriptPath
    {
        get => _postTransferScriptPath;
        set { _postTransferScriptPath = value; OnPropertyChanged(); }
    }

    private bool _runRemoteTransfer;
    public bool RunRemoteTransfer
    {
        get => _runRemoteTransfer;
        set { _runRemoteTransfer = value; OnPropertyChanged(); }
    }

    private bool _runRemoteScript;
    public bool RunRemoteScript
    {
        get => _runRemoteScript;
        set { _runRemoteScript = value; OnPropertyChanged(); }
    }
}
