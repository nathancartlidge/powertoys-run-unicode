using Microsoft.CommandPalette.Extensions.Toolkit;

#nullable enable
namespace UnicodeInputExtension.Helpers
{
    public partial class ClipboardCommand : InvokableCommand
    {
        private readonly string _data;

        private CommandResult Result { get; } = CommandResult.Hide();

        public ClipboardCommand(string data)
        {
            _data = data;
            Name = "Copy Symbol";
            Icon = new IconInfo("\uE8C8");
        }
        
        public ClipboardCommand(string data, string name)
        {
            _data = data;
            Name = name;
            Icon = new IconInfo("\uE8C8");
        }
        
        public override CommandResult Invoke()
        {
            ClipboardHelper.SetText(_data);
            return Result;
        }
    }
}