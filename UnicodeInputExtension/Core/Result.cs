using System.Collections.Generic;

namespace UnicodeInputExtension.Core;

public struct Result(
    string UserInput,
    int? ResultIndex,
    List<string> Choices,
    List<char> ValidNextChars,
    int Score,
    bool IsHtml = false
);
