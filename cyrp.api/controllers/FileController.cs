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
        private OpenAIService _openAIService;

        public FileController(OpenAIService openAIService)
        {
            this._openAIService = openAIService;
        }

        [HttpGet("uploadfile")]
        //[RequestSizeLimit(MaxFileSize)]
        public async Task<ActionResult> UploadFile(
            [FromForm] IFormFile? file,
            CancellationToken cancellationToken
        )
        {
            try
            {
                // TODO 1: file data conversion to text so i can push trough API 
                    // TODO: Validate allowed types
                    // TODO: Create processors for every data conversion flow (audio, text and video)
                            // TODO: Audio transcript via OpenAI                    
                            // TODO: Text analysis
                            // TODO: extracting audio and frames


                // TODO 2: Transformed data adjustments for request send

                // TODO 3: 

                var res = await _openAIService.AskSync("testing....");
                return Ok(res);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}