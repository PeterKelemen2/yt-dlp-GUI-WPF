using System;
using System.Globalization;
using System.Resources;
using System.Windows.Markup;

namespace YtDlpGuiWpf
{
    public class LocExtension : MarkupExtension
    {
        private static readonly ResourceManager ResourceManager = Resources.Strings.ResourceManager;

        public string Key { get; set; }

        public LocExtension(string key) {  Key = key; }
        
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Key == null)  return "[null key]";
    
            return Loc.Get(Key);
        }
    }
}