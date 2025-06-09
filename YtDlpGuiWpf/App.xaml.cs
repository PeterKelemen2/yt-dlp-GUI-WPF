using System.Configuration;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace YtDlpGuiWpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    // protected override void OnStartup(StartupEventArgs e)
    // {
    //     Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
    //     Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
    //     base.OnStartup(e);
    // }
    
    private void ComboBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is ComboBox combo && !combo.IsDropDownOpen)
        {
            combo.IsDropDownOpen = true;
            e.Handled = true; // Optional: prevents other click handling
        }
    }

}