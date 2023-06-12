using System;
using System.Text.RegularExpressions;
using Godot;
using tiktaktoe.Messages;

namespace tiktaktoe.Utils;

public static partial class Validators
{
    public const int Limit = 1;
    public const bool Authoritative = true;
    public const int MaxCode = 999_999;
    public const int MinCode = 100_000;

    public static int GetMaxPlayers(Level level) => level switch
    {
        Level.Classic => 2,
        Level.Adventure => Enum.GetValuesAsUnderlyingType<Mark>().Length,
        _ => 2
    };

    public static bool VerifyJoinCode(this double joinCode)
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        => Math.Floor(joinCode) == joinCode && joinCode is >= MinCode and <= MaxCode;

    // https://github.com/heroiclabs/nakama/blob/c7d34a89c6ecb7c1ef4757de5881a77f4f2a6e3e/server/api_authenticate.go#L449-L451
    public static bool VerifyUsername(this LineEdit username, string? usernameText = null)
    {
        var finalUsername = usernameText ?? username.Text;
        if (finalUsername.Length <= 128 && UsernameRegex().IsMatch(finalUsername)) return true;
        username.SetFontColor(Colors.Red);
        return false;
    }

    public static bool VerifyEmail(this LineEdit email, string? emailText = null)
    {
        var finalEmail = emailText ?? email.Text;
        if (finalEmail.Length is >= 10 and <= 255 && finalEmail.IsEmail()) return true;
        email.SetFontColor(Colors.Red);
        return false;
    }

    public static bool VerifyPassword(this LineEdit password, string? passwordText = null)
    {
        var finalPassword = passwordText ?? password.Text;
        if (finalPassword.Length > 6) return true;
        password.SetFontColor(Colors.Red);
        return false;
    }

    public static bool IsEmail(this string text)
        => EmailRegex().IsMatch(text);

    // RFC 2822: https://learn.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format
    [GeneratedRegex(
        @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z",
        RegexOptions.IgnoreCase, "en-US")]
    public static partial Regex EmailRegex();

    [GeneratedRegex(@"^([a-z\d]+(\.|_))*[a-z\d]+$", RegexOptions.IgnoreCase)]
    public static partial Regex UsernameRegex();
}