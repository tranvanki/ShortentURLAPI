namespace ShortenURLService.Models
{
    public class URL
    {
        public int Id { get; set; }
        public string OriginalUrl { get; set; } = string.Empty;
        public string ShortenedUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public String? metadata { get; set; } = string.Empty;
    }
}
