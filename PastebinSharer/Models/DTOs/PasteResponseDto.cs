namespace PastebinSharer.Models.DTOs
{
    public class PasteResponseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Language { get; set; } = "plaintext";
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsPrivate { get; set; }
        public int ViewCount { get; set; }
    }
}