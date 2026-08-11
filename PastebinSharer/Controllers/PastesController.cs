using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PastebinSharer.Models.DTOs;
using PastebinSharer.Services;

namespace PastebinSharer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PastesController : ControllerBase
    {
        private readonly PasteService _pasteService;

        public PastesController(PasteService pasteService)
        {
            _pasteService = pasteService;
        }

        // POST /api/pastes - Tạo một Paste mới (Tự động nhận diện OwnerId nếu đã đăng nhập)
        [HttpPost]
        public async Task<ActionResult<PasteResponseDto>> CreatePaste([FromBody] CreatePasteDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            int? userId = null;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("id")?.Value
                              ?? User.FindFirst("sub")?.Value
                              ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int parsedUserId))
            {
                userId = parsedUserId;
            }

            var response = await _pasteService.CreatePasteAsync(dto, userId);

            return CreatedAtAction(nameof(GetPaste), new { code = response.Code }, response);
        }

        // GET /api/pastes/my - Lấy danh sách Paste của người dùng đang đăng nhập
        [HttpGet("my")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<PasteResponseDto>>> GetMyPastes()
        {
   
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? User.FindFirst("id")?.Value
                              ?? User.FindFirst("sub")?.Value
                              ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Không xác thực được danh tính người dùng." });
            }

            var responses = await _pasteService.GetPastesByUserIdAsync(userId);
            return Ok(responses);
        }

        // GET /api/pastes/{code} - Lấy thông tin Paste theo mã Code (Tự động tăng ViewCount)
        [HttpGet("{code}")]
        public async Task<ActionResult<PasteResponseDto>> GetPaste(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return BadRequest(new { message = "Mã paste không hợp lệ." });
            }

            var response = await _pasteService.GetPasteByCodeAsync(code.Trim());

            if (response == null)
            {
                return NotFound(new { message = "Paste không tồn tại hoặc đã hết hạn" });
            }

            return Ok(response);
        }

        // GET /api/pastes - Lấy danh sách các Paste công khai
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PasteResponseDto>>> GetPublicPastes()
        {
            var responses = await _pasteService.GetPublicPastesAsync();
            return Ok(responses);
        }

        // DELETE /api/pastes/{code} - Xóa Paste theo mã Code
        [HttpDelete("{code}")]
        public async Task<IActionResult> DeletePaste(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return BadRequest(new { message = "Mã paste không hợp lệ." });
            }

            var isDeleted = await _pasteService.DeletePasteAsync(code.Trim());

            if (!isDeleted)
            {
                return NotFound(new { message = "Paste không tồn tại hoặc đã bị xóa trước đó." });
            }

            return Ok(new { message = $"Đã xóa thành công Paste có mã '{code}'" });
        }
    }
}