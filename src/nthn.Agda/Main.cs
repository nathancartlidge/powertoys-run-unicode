using ManagedCommon;
using System.Collections.Generic;
using System.Windows.Forms;
using Wox.Plugin;

namespace nthn.Agda
{
    public class Main : IPlugin
    {
        private string IconPath { get; set; }

        private PluginInitContext Context { get; set; }
        public string Name => "Agda";

        public string Description => "Agda Unicode Input";

        public static string PluginID => "778f24fc48714097b30303f83d5bed6a";

        public List<Result> Query(Query query)
        {
            return new List<Result>
            {
                new Result
                {
                    Title = "Match found!",
                    SubTitle = "Copy this symbol to the clipboard",
                    IcoPath = IconPath,
                    Action = e =>
                    {
                        Clipboard.SetText("≡");
                        return true;
                    },
                }
            };
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
