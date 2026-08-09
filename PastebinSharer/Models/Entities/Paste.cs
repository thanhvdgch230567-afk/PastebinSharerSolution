using System;

namespace PastebinSharer.Entities
{
    public class Paste
    {
        public int Id { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string Language { get; set; } = "plaintext";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiresAt { get; set; }

        public bool IsPrivate { get; set; } = false;

        public int ViewCount { get; set; } = 0;

        // 🟢 Sửa int? thành string? để Npgsql đọc kiểu text/varchar từ PostgreSQL không bị ném lỗi GetInt32
        public string? OwnerId { get; set; }
    }
}