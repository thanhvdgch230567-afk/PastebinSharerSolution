using Microsoft.EntityFrameworkCore;
using PastebinSharer.Data;
using PastebinSharer.Entities;
using PastebinSharer.Helpers;
using PastebinSharer.Models.DTOs;

namespace PastebinSharer.Services
{
    public class PasteService
    {
        private readonly AuthDbContext _context;

        public PasteService(AuthDbContext context)
        {
            _context = context;
        }

        // 1. Logic Tạo Paste mới
        public async Task<PasteResponseDto> CreatePasteAsync(CreatePasteDto dto, int? userId = null)
        {
            string code;
            do
            {
                code = CodeGenerator.GenerateCode(6);
            }
            while (await _context.Pastes.AnyAsync(p => p.Code == code));

            DateTime? expiresAt = dto.Expiration?.ToLower() switch
            {
                "1h" => DateTime.UtcNow.AddHours(1),
                "1d" => DateTime.UtcNow.AddDays(1),
                "1w" => DateTime.UtcNow.AddDays(7),
                _ => null
            };

            var paste = new Paste
            {
                Code = code,
                Content = dto.Content,
                Language = string.IsNullOrWhiteSpace(dto.Language) ? "plaintext" : dto.Language,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                IsPrivate = dto.IsPrivate,
                ViewCount = 0,
                OwnerId = userId?.ToString() 
            };

            _context.Pastes.Add(paste);
            await _context.SaveChangesAsync();

            return MapToResponseDto(paste);
        }

        // 2. Logic Lấy thông tin Paste theo Code
        public async Task<PasteResponseDto?> GetPasteByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;

            var cleanCode = code.Trim();

            // Tìm paste theo code
            var paste = await _context.Pastes.FirstOrDefaultAsync(p => p.Code == cleanCode);

            if (paste == null) return null;

            // Kiểm tra hết hạn
            if (paste.ExpiresAt.HasValue && paste.ExpiresAt.Value < DateTime.UtcNow)
            {
                return null;
            }

            // Tăng ViewCount an toàn
            try
            {
                paste.ViewCount++;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Warning] Lỗi tăng ViewCount: {ex.Message}");
            }

            return MapToResponseDto(paste);
        }

        // 3. Logic Lấy danh sách các Paste công khai
        public async Task<IEnumerable<PasteResponseDto>> GetPublicPastesAsync()
        {
            var pastes = await _context.Pastes
                .AsNoTracking()
                .Where(p => !p.IsPrivate)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var now = DateTime.UtcNow;
            return pastes
                .Where(p => p.ExpiresAt == null || p.ExpiresAt > now)
                .Select(p => MapToResponseDto(p));
        }

        // 4. Logic Lấy danh sách Paste theo OwnerId
        public async Task<IEnumerable<PasteResponseDto>> GetPastesByUserIdAsync(int userId)
        {
            string userIdStr = userId.ToString();

            var pastes = await _context.Pastes
                .AsNoTracking()
                .Where(p => p.OwnerId == userIdStr)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var now = DateTime.UtcNow;
            return pastes
                .Where(p => p.ExpiresAt == null || p.ExpiresAt > now)
                .Select(p => MapToResponseDto(p));
        }

        // 5. Logic Xóa Paste theo Code
        public async Task<bool> DeletePasteAsync(string code)
        {
            var paste = await _context.Pastes.FirstOrDefaultAsync(p => p.Code == code);

            if (paste == null) return false;

            _context.Pastes.Remove(paste);
            await _context.SaveChangesAsync();
            return true;
        }

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