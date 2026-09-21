using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace UrlShortener.Data;

public class Url
{
    public int Id { get; set; }
    public required string LongUrl { get; set; }
    public string? ShortUrl { get; set; }
    [Required]
    public int ManagerId { get; set; }
    public int RedirectCount { get; set; }
    
    [Timestamp]
    public uint Version { get; set; }
}