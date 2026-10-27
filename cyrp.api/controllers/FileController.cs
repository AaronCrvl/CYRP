using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    [Route("files")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private const long MaxFileSize = 25 * 1024 * 1024;
        //private OpenAIService _openAIService;

        public FileController()
        {
        }

        [HttpPost("uploadfile")]
        [RequestSizeLimit(MaxFileSize)]
        public async Task<ActionResult> UploadFile(
            [FromForm] IFormFile? file
        )
        {
            try
            {
                await Task.CompletedTask;
                return Ok("Testing");
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}