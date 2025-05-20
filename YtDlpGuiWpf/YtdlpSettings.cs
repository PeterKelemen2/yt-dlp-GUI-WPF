using System.IO;
using System.Text.Json;

namespace YtDlpGuiWpf;

public class YtdlpSettings
{
    public const string ConfigPath = "config.json";
    
    public string YtDlpPath { get; set; }
    public string LocalSavePath { get; set; }
    public string YtDlpArguments { get; set; }
    public bool EnablePostInstall { get; set; }
    public string RemoteUsername { get; set; }
    public string RemotePassword { get; set; }
    public string RemoteLocation { get; set; }
    public string PostTransferScriptPath { get; set; }
    public bool RunRemoteTransfer { get; set; }
    public bool RunRemoteScript { get; set; }
}
