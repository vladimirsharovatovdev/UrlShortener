namespace UrlShortener.Data.Services;

public class AuthSettings
{
    public string Secret {  get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int ValidSeconds { get; set; }
}