using Microsoft.CommandPalette.Extensions.Toolkit;

#nullable enable
namespace UnicodeInputExtension.Helpers
{
    public partial class ClipboardCommand : InvokableCommand
    {
        private readonly string _data;

        public CommandResult Result { get; set; } = CommandResult.KeepOpen();

        public ClipboardCommand(string data)
        {
            _data = data;
            Name = "Copy to clipboard";
            Icon = new IconInfo("\uE8C8");
        }
        
        public override CommandResult Invoke()
        {
            ClipboardHelper.SetText(_data);
            return Result;
        }
    }
}
