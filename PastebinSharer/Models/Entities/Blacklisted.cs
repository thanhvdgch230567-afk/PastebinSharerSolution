namespace PastebinSharer.Entities
{
    public class BlacklistedToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}