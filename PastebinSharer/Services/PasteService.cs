using Microsoft.EntityFrameworkCore;
using PastebinSharer.Data;
using PastebinSharer.Entities;
using PastebinSharer.Helpers;
using PastebinSharer.Models.DTOs;

namespace PastebinSharer.Services
{
    public class PasteService
    {
        private readonly ApplicationDbContext _context;

        public PasteService(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Logic Tạo Paste mới
        public async Task<PasteResponseDto> CreatePasteAsync(CreatePasteDto dto)
        {
            // Sinh code ngẫu nhiên duy nhất
            string code;
            do
            {
                code = CodeGenerator.GenerateCode(6);
            }
            while (await _context.Pastes.AnyAsync(p => p.Code == code));

            // Tính thời gian hết hạn (ExpiresAt)
            DateTime? expiresAt = dto.Expiration?.ToLower() switch
            {
                "1h" => DateTime.UtcNow.AddHours(1),
                "1d" => DateTime.UtcNow.AddDays(1),
                "1w" => DateTime.UtcNow.AddDays(7),
                _ => null // "never" hoặc mặc định
            };

            var paste = new Paste
            {
                Code = code,
                Content = dto.Content,
                Language = string.IsNullOrWhiteSpace(dto.Language) ? "plaintext" : dto.Language,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                IsPrivate = dto.IsPrivate,
                ViewCount = 0
            };

            _context.Pastes.Add(paste);
            await _context.SaveChangesAsync();

            return MapToResponseDto(paste);
        }

        // 2. Logic Lấy thông tin Paste theo Code (Tự động tăng ViewCount)
        public async Task<PasteResponseDto?> GetPasteByCodeAsync(string code)
        {
            var paste = await _context.Pastes.FirstOrDefaultAsync(p => p.Code == code);

            if (paste == null) return null;

            // Kiểm tra nếu Paste đã hết hạn
            if (paste.ExpiresAt.HasValue && paste.ExpiresAt.Value < DateTime.UtcNow)
            {
                return null; // Đã hết hạn
            }

            // Tăng lượt xem
            paste.ViewCount++;
            await _context.SaveChangesAsync();

            return MapToResponseDto(paste);
        }

        // 3. Logic Lấy danh sách các Paste công khai (Đã SỬA LỖI LINQ EF CORE)
        public async Task<IEnumerable<PasteResponseDto>> GetPublicPastesAsync()
        {
            var now = DateTime.UtcNow;

            // Dùng Select chiếu trực tiếp sang DTO để EF Core dịch chuẩn sang SQL
            return await _context.Pastes
                .AsNoTracking()
                .Where(p => !p.IsPrivate && (p.ExpiresAt == null || p.ExpiresAt > now))
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PasteResponseDto
                {
                    Code = p.Code,
                    Content = p.Content,
                    Language = p.Language,
                    CreatedAt = p.CreatedAt,
                    ExpiresAt = p.ExpiresAt,
                    IsPrivate = p.IsPrivate,
                    ViewCount = p.ViewCount
                })
                .ToListAsync();
        }

        // 4. Logic Xóa Paste theo Code
        public async Task<bool> DeletePasteAsync(string code)
        {
            var paste = await _context.Pastes.FirstOrDefaultAsync(p => p.Code == code);

            if (paste == null) return false;

            _context.Pastes.Remove(paste);
            await _context.SaveChangesAsync();
            return true;
        }

        // Helper Map Entity sang Response DTO trong Memory
        private static PasteResponseDto MapToResponseDto(Paste paste)
        {
            return new PasteResponseDto
            {
                Code = paste.Code,
                Content = paste.Content,
                Language = paste.Language,
                CreatedAt = paste.CreatedAt,
                ExpiresAt = paste.ExpiresAt,
                IsPrivate = paste.IsPrivate,
                ViewCount = paste.ViewCount
            };
        }
    }
}
