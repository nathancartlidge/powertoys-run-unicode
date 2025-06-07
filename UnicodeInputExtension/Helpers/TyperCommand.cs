using System.Threading;
using Microsoft.CommandPalette.Extensions.Toolkit;

using WindowsInput;

// Modified from the original implementation by Corey Hayward, licenced under MIT
// https://github.com/CoreyHayward/PowerToys-Run-InputTyper/blob/9daf6b50ca742f58ef44ddb6325797dd92b2b308/Community.PowerToys.Run.Plugin.InputTyper/Typer.cs

#nullable enable
namespace UnicodeInputExtension.Helpers
{
    public partial class TyperCommand : InvokableCommand
    {
        private readonly string _data;
        private const int Delay = 200;

        private CommandResult Result { get; } = CommandResult.Hide();

        public TyperCommand(string data)
        {
            _data = data;
            Name = "Type characters";
            Icon = new IconInfo("\uE765");
        }

        private void Type()
        {
            Thread.Sleep(Delay);
            InputSimulator sim = new();
            sim.Keyboard.TextEntry(_data); 
        }
        
        public override CommandResult Invoke()
        {
            var thread = new Thread(Type);
            thread.Start();
            return Result;
        }
    }
}
