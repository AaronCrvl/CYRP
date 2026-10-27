using System.Text.Json;

public class OpenAIService
{
    private readonly string? _apiKey;
    private readonly string _model;

    public OpenAIService(IConfiguration configuration)
    {
        _apiKey = configuration["OpenAI:ApiKey"];
        _model = configuration["OpenAI:Model"] ?? "gpt-4.1-mini";
    }

    public async Task<string?> AskSync(
        IFormFile[] files,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new InvalidOperationException(
                "OpenAI API key is missing. Configure OpenAI:ApiKey through user secrets or environment variables before calling this service.");
        }

        var client = new HttpClient();
        client.BaseAddress = new Uri("https://api.openai.com/v1/audio/transcriptions");                
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue($"Bearer", _apiKey);

        var promptModel = new OpenAiSerivce.Models.OpenAiRequestPromptModel(null, null);
        promptModel.transcriptions += "/n/f" + files.Select(async (file) =>
        {
            using var content = new MultipartFormDataContent();
            await using var stream = file.OpenReadStream();
            content.Add(new StreamContent(stream), "file", file.FileName);
            content.Add(new StringContent("whisper-1"), "model");
            content.Add(new StringContent("text"), "response_format");
            return content.ToString();
        });

        var jsonPrompt = JsonSerializer.Serialize(promptModel);
        var transcriptContent = new MultipartContent();
        transcriptContent.Add(new StringContent(promptModel.transcriptions.ToString()));
        
        var response = await client.PostAsync(
        "https://api.openai.com/v1/audio/transcriptions", transcriptContent);

        response.EnsureSuccessStatusCode();
        var res = await response.Content.ReadAsStringAsync();
        return res;
    }
}