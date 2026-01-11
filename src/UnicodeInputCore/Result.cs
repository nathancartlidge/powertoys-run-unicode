using System.Collections.Generic;


namespace UnicodeInput.Core;

public record struct Result(
    string UserInput,
    int? ResultIndex,
    List<string> Choices,
    List<char> ValidNextChars,
    int Score,
    string Sources
);
