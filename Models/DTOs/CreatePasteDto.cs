using System.ComponentModel.DataAnnotations;

namespace PastebinSharer.Models.DTOs
{
    public class CreatePasteDto
    {
        [Required(ErrorMessage = "Nội dung paste không được để rỗng")]
        [MaxLength(512000, ErrorMessage = "Kích thước paste tối đa là 500 KB")] // Áp dụng giới hạn kích thước 500KB
        public string Content { get; set; } = string.Empty;

        public string Language { get; set; } = "plaintext";

        // Nhận vào các tùy chọn: "1h", "1d", "1w", "never"
        public string Expiration { get; set; } = "never";

        public bool IsPrivate { get; set; } = false;
    }
}