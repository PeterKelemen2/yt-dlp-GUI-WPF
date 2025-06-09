using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace YtDlpGuiWpf;

public class LocalizationViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    // Helper to raise PropertyChanged
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public string VideoUrl => Loc.Get("videoUrl");
    public string BPaste => Loc.Get("bPaste");
    public string LocalSavePath => Loc.Get("localSavePath");
    public string BBrowse => Loc.Get("bBrowse");
    public string YtDlpInstallPath => Loc.Get("ytDlpInstallPath");
    public string YtDlpArgs => Loc.Get("ytDlpArgs");
    public string YtDlpNaming => Loc.Get("ytDlpNaming");
    public string PostEnablePostActions => Loc.Get("postEnablePostActions");
    public string PostTransToRemote => Loc.Get("postTransToRemote");
    public string PostRunRemoteScript => Loc.Get("postRunRemoteScript");
    public string RemoteServerConfTitle => Loc.Get("remoteServerConfTitle");
    public string RHost => Loc.Get("rHost");
    public string RUser => Loc.Get("rUser");
    public string RPassword => Loc.Get("rPassword");
    public string RRemotePath => Loc.Get("rRemotePath");
    public string RRemoteScriptPath => Loc.Get("rRemoteScriptPath");
    public string ROs => Loc.Get("rOs");
    public string BDownload => Loc.Get("bDownload");
    public string BLang => Loc.Get("lang");
    
    // Call this method after culture changes to update all bindings
    public void RaiseAllPropertiesChanged()
    {
        OnPropertyChanged(nameof(VideoUrl));
        OnPropertyChanged(nameof(BPaste));
        OnPropertyChanged(nameof(LocalSavePath));
        OnPropertyChanged(nameof(BBrowse));
        OnPropertyChanged(nameof(YtDlpInstallPath));
        OnPropertyChanged(nameof(YtDlpArgs));
        OnPropertyChanged(nameof(YtDlpNaming));
        OnPropertyChanged(nameof(PostEnablePostActions));
        OnPropertyChanged(nameof(PostTransToRemote));
        OnPropertyChanged(nameof(PostRunRemoteScript));
        OnPropertyChanged(nameof(RemoteServerConfTitle));
        OnPropertyChanged(nameof(RHost));
        OnPropertyChanged(nameof(RUser));
        OnPropertyChanged(nameof(RPassword));
        OnPropertyChanged(nameof(RRemotePath));
        OnPropertyChanged(nameof(RRemoteScriptPath));
        OnPropertyChanged(nameof(ROs));
        OnPropertyChanged(nameof(BDownload));
        OnPropertyChanged(nameof(BLang));
    }
}