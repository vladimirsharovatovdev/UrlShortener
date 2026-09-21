namespace UrlShortener.Data;

public class Manager
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string PasswordHash { get; set; }
    public List<Url> AssociatedUrls { get; set; }
}