namespace Community.PowerToys.Run.Plugin.UnicodeInput;

// Slightly modified from the original implementation by Corey Hayward, licenced under MIT
// https://github.com/CoreyHayward/PowerToys-Run-InputTyper/blob/9daf6b50ca742f58ef44ddb6325797dd92b2b308/Community.PowerToys.Run.Plugin.InputTyper/Typer.cs

internal sealed class Typer
{
    private readonly char[] _specialCharacters = { '{', '}', '+', '^', '%', '~', '(', ')'  };
    private const int Interkeydelay = 20;
    
    public void Type(string str, int delay = 200)
    {
        Thread.Sleep(delay);
        foreach (var c in str.ToCharArray())
        {
            // Some characters have special meaning and must be surrounded by '{}'
            // https://docs.microsoft.com/en-us/office/vba/language/reference/user-interface-help/sendkeys-statement
            if (_specialCharacters.Contains(c))
            {
                SendKeys.SendWait("{" + c + "}");
            }
            else
            {
                SendKeys.SendWait(c.ToString());
            }
               
            Thread.Sleep(Interkeydelay);
        }
    }
}