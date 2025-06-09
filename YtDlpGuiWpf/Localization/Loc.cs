using System.Globalization;
using System.Resources;

namespace YtDlpGuiWpf;

public class Loc
{
    private static readonly ResourceManager _resourceManager = Resources.Strings.ResourceManager;
    private static CultureInfo _currentCulture = CultureInfo.CurrentUICulture;
    
    public static string Get(string key)
    {
        return _resourceManager.GetString(key, _currentCulture) ?? $"!{key}!";
    }
    
    // Method to switch culture
    public static void SetCulture(string cultureName)
    {
        _currentCulture = new CultureInfo(cultureName);

        // Update system-wide culture as well
        CultureInfo.DefaultThreadCurrentCulture = _currentCulture;
        CultureInfo.DefaultThreadCurrentUICulture = _currentCulture;
    }

    public static CultureInfo GetCurrentCulture() => _currentCulture;
}