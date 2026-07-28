using Microsoft.AspNetCore.Mvc;
using PastebinSharer.Models.DTOs;
using PastebinSharer.Services;
using System.Diagnostics.CodeAnalysis;

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

        // POST /api/pastes
        [HttpPost]
        public async Task<ActionResult<PasteResponseDto>> CreatePaste([FromBody] CreatePasteDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _pasteService.CreatePasteAsync(dto);
            return CreatedAtAction(nameof(GetPaste), new { code = response.Code }, response);
        }

        // GET /api/pastes/{code}
        [HttpGet("{code}")]
        public async Task<ActionResult<PasteResponseDto>> GetPaste(string code)
        {
            var response = await _pasteService.GetPasteByCodeAsync(code);

            if (response == null)
            {
                return NotFound(new { message = "Paste không tồn tại hoặc đã hết hạn" });
            }

            return Ok(response);
        }

        // GET /api/pastes (API MỞ RỘNG 1: Lấy danh sách Paste công khai)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PasteResponseDto>>> GetPublicPastes()
        {
            var responses = await _pasteService.GetPublicPastesAsync();
            return Ok(responses);
        }

        // DELETE /api/pastes/{code} (API MỞ RỘNG 2: Xóa Paste)
        [HttpDelete("{code}")]
        public async Task<IActionResult> DeletePaste(string code)
        {
            var isDeleted = await _pasteService.DeletePasteAsync(code);

            if (!isDeleted)
            {
                return NotFound(new { message = "Paste không tồn tại để xóa" });
            }

            return Ok(new { message = $"Đã xóa thành công Paste có mã '{code}'" });
        }
    }
}   