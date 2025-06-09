using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace YtDlpGuiWpf;

public class YtdlpSettings : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    [JsonIgnore] public List<string> AvailableOSList { get; } = new() { "Linux", "Windows" };

    private string _culture = "en";

    public string Culture
    {
        get => _culture;
        set
        {
            if (_culture != value)
            {
                _culture = value;
                OnPropertyChanged();
            }
        }
    }

    private string _localSavePath = string.Empty;

    public string LocalSavePath
    {
        get => _localSavePath;
        set
        {
            _localSavePath = value;
            OnPropertyChanged();
        }
    }

    private string _ytDlpPath = string.Empty;

    public string YtDlpPath
    {
        get => _ytDlpPath;
        set
        {
            _ytDlpPath = value;
            OnPropertyChanged();
        }
    }


    private string _ytDlpArguments = "-x --audio-format mp3 --embed-thumbnail --add-metadata";

    public string YtDlpArguments
    {
        get => _ytDlpArguments;
        set
        {
            _ytDlpArguments = value;
            OnPropertyChanged();
        }
    }

    private string _ytDlpNamingScheme = "%(track, title, id)s.%(ext)s";

    public string YtDlpNamingScheme
    {
        get => _ytDlpNamingScheme;
        set
        {
            _ytDlpNamingScheme = value;
            OnPropertyChanged();
        }
    }

    private bool _enablePostInstall;

    public bool EnablePostInstall
    {
        get => _enablePostInstall;
        set
        {
            _enablePostInstall = value;
            OnPropertyChanged();
        }
    }

    private string _remoteHost = string.Empty;

    public string RemoteHost
    {
        get => _remoteHost;
        set
        {
            _remoteHost = value;
            OnPropertyChanged();
        }
    }

    private string _remoteUsername = string.Empty;

    public string RemoteUsername
    {
        get => _remoteUsername;
        set
        {
            _remoteUsername = value;
            OnPropertyChanged();
        }
    }

    private string _remotePassword = string.Empty;

    public string RemotePassword
    {
        get => _remotePassword;
        set
        {
            _remotePassword = value;
            OnPropertyChanged();
        }
    }

    private string _remoteLocation = string.Empty;

    public string RemoteLocation
    {
        get => _remoteLocation;
        set
        {
            _remoteLocation = value;
            OnPropertyChanged();
        }
    }

    private string _selectedRemoteOS = "Linux"; // default

    public string SelectedRemoteOS
    {
        get => _selectedRemoteOS;
        set
        {
            _selectedRemoteOS = value;
            OnPropertyChanged(nameof(SelectedRemoteOS));
        }
    }

    private string _postTransferScriptPath = string.Empty;

    public string PostTransferScriptPath
    {
        get => _postTransferScriptPath;
        set
        {
            _postTransferScriptPath = value;
            OnPropertyChanged();
        }
    }

    private bool _runRemoteTransfer;

    public bool RunRemoteTransfer
    {
        get => _runRemoteTransfer;
        set
        {
            _runRemoteTransfer = value;
            OnPropertyChanged();
        }
    }

    private bool _runRemoteScript;

    public bool RunRemoteScript
    {
        get => _runRemoteScript;
        set
        {
            _runRemoteScript = value;
            OnPropertyChanged();
        }
    }

    private bool _remoteBlockOpen = false;

    public bool RemoteBlockOpen
    {
        get => _remoteBlockOpen;
        set
        {
            _remoteBlockOpen = value;
            OnPropertyChanged();
        }
    }
}