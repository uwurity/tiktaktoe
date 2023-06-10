using System.Text.RegularExpressions;

namespace tiktaktoe.Utils;

public static partial class Validators
{
    // RFC 2822: https://learn.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format
    [GeneratedRegex(
        "\\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\\Z",
        RegexOptions.IgnoreCase, "en-US")]
    public static partial Regex EmailRegex();
}
