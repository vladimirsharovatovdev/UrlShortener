namespace UrlShortener.Data.Services;

/// <summary>
/// Validation rules for user-supplied short codes and destination URLs.
/// Used at the API boundary; internal tools (e.g. the DB populator) may bypass it.
/// </summary>
public static class UrlValidator
{
    public const int MaxShortUrlLength = 64;
    public const int MaxLongUrlLength = 2048;

    /// <summary>A short code is 1..64 characters: ASCII letters, digits, '-' and '_'.</summary>
    public static bool IsValidShortUrl(string? shortUrl)
    {
        if (string.IsNullOrEmpty(shortUrl) || shortUrl.Length > MaxShortUrlLength)
            return false;

        foreach (var c in shortUrl)
        {
            if (!(char.IsAsciiLetterOrDigit(c) || c is '-' or '_'))
                return false;
        }

        return true;
    }

    /// <summary>A destination must be an absolute http(s) URL with a host, no whitespace or control characters.</summary>
    public static bool IsValidLongUrl(string? longUrl)
    {
        if (string.IsNullOrEmpty(longUrl) || longUrl.Length > MaxLongUrlLength)
            return false;

        foreach (var c in longUrl)
        {
            if (char.IsWhiteSpace(c) || char.IsControl(c))
                return false;
        }

        return Uri.TryCreate(longUrl, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
               && !string.IsNullOrEmpty(uri.Host);
    }

    /// <summary>Returns an error message, or null when the destination URL is valid.</summary>
    public static string? ValidateLongUrl(string? longUrl)
    {
        if (longUrl is { Length: > MaxLongUrlLength })
            return $"The URL is too long (max {MaxLongUrlLength} characters).";

        return IsValidLongUrl(longUrl)
            ? null
            : "Enter a full URL starting with http:// or https:// (no spaces).";
    }

    /// <summary>
    /// Returns an error message, or null when the short code is valid.
    /// An empty code is allowed: the service then generates one.
    /// </summary>
    public static string? ValidateShortUrl(string? shortUrl)
    {
        if (string.IsNullOrEmpty(shortUrl))
            return null;

        if (shortUrl.Length > MaxShortUrlLength)
            return $"The short code is too long (max {MaxShortUrlLength} characters).";

        return IsValidShortUrl(shortUrl)
            ? null
            : "The short code may contain only letters, numbers, '-' and '_'.";
    }
}
