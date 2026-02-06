using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MESSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        [HttpPost("open")]
        public IActionResult OpenFile([FromBody] OpenFileRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.FilePath))
                {
                    return BadRequest(new { error = "파일 경로가 제공되지 않았습니다." });
                }

                // 파일 또는 폴더 경로 검증
                bool isFile = System.IO.File.Exists(request.FilePath);
                bool isDirectory = System.IO.Directory.Exists(request.FilePath);

                if (!isFile && !isDirectory)
                {
                    return NotFound(new { error = "파일 또는 폴더를 찾을 수 없습니다.", filePath = request.FilePath });
                }

                // Windows에서 탐색기로 열기
                var startInfo = new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    UseShellExecute = true
                };

                if (isFile)
                {
                    // 파일인 경우: 파일 선택하여 열기
                    startInfo.Arguments = $"/select,\"{request.FilePath}\"";
                }
                else
                {
                    // 폴더인 경우: 폴더를 직접 열기
                    startInfo.Arguments = $"\"{request.FilePath}\"";
                }

                Process.Start(startInfo);

                return Ok(new { success = true, message = isFile ? "파일 탐색기에서 파일을 열었습니다." : "폴더를 열었습니다.", filePath = request.FilePath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "파일을 여는 중 오류가 발생했습니다.", message = ex.Message });
            }
        }
    }

    public class OpenFileRequest
    {
        public string FilePath { get; set; } = string.Empty;
    }
}
