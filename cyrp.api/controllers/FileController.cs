using Microsoft.AspNetCore.Mvc;
using OpenAISrvc;
using OpenAISrvc.Schemas;

namespace Controllers
{
    [Route("files")]
    [ApiController]
    public sealed class FileController : ControllerBase
    {
        private const long MaxFileSize = 25 * 1024 * 1024;
        private readonly OpenAIService _openAIService;

        public FileController(OpenAIService openAISerivce)
        {
            this._openAIService = openAISerivce;
        }

        [HttpPost("uploadfile")]
        [RequestSizeLimit(MaxFileSize)]
        public async Task<ActionResult<AudioAnalysis>> UploadFile(
            [FromForm] IFormFile[]? files,
            CancellationToken cancellationToken
        )
        {
            if (files == null || files.Length == 0)
                return BadRequest("No files were added to request");

            var transcription = await _openAIService.TranscribeAsync(files, cancellationToken);
            var analysis = await _openAIService.AskingForDirections(transcription, cancellationToken);
            return Ok(analysis);
        }
    }
}