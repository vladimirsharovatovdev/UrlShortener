namespace UrlShortener.Data;

public class RefreshTokenEntry
{
    public int Id {  get; set; }
    public int ManagerId { get; set; }
    public Manager Manager { get; set; }
    public string AuthToken { get; set; }
    public string RefreshToken { get; set; }
}