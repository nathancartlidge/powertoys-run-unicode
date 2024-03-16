using ManagedCommon;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Windows.Forms;
using Wox.Plugin;

namespace nthn.Agda
{
    public class Main : IPlugin
    {
        private string IconPath { get; set; }
        private AgdaLookup lookup = new AgdaLookup();

        private PluginInitContext Context { get; set; }
        public string Name => "Agda";

        public string Description => "Agda Unicode Input";

        public static string PluginID => "778f24fc48714097b30303f83d5bed6a";

        public List<Result> Query(Query query)
        {
            string q = query.RawQuery.Substring(1).Trim();
            
            if (lookup.keyValuePairs.ContainsKey(q))
            {
                string value = lookup.keyValuePairs.GetValueOrDefault(q, "");
                List<Result> results = new List<Result>();
                for (int i = 0; i < value.Length; i++)
                {
                    results.Add(
                        new Result
                        {
                            Title = value[i].ToString(),
                            SubTitle = "Copy this symbol to the clipboard",
                            IcoPath = IconPath,
                            Action = e =>
                            {
                                Clipboard.SetText(value.ToString());
                                return true;
                            },
                        }
                    ); ;
                }
                return results;
            }
            return new List<Result> { };
        }

        public void Init(PluginInitContext context)
        {
            Context = context;
            Context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(Context.API.GetCurrentTheme());
        }

        private void UpdateIconPath(Theme theme)
        {
            if (theme == Theme.Light || theme == Theme.HighContrastWhite)
            {
                IconPath = "images/agda.light.png";
            }
            else
            {
                IconPath = "images/agda.dark.png";
            }
        }

        private void OnThemeChanged(Theme currentTheme, Theme newTheme)
        {
            UpdateIconPath(newTheme);
        }
    }
}
