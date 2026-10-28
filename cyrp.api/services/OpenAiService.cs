using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using OpenAISrvc.Models;
using OpenAISrvc.Schemas;

namespace OpenAISrvc
{
    public sealed class OpenAIService
    {
        private readonly HttpClient _client;
        private readonly string? _apiKey;
        private readonly string[] audioTypes = {
            "audio/mpeg", "audio/mp3", "audio/wav",
            "audio/x-wav", "audio/wave", "audio/mp4",
            "audio/x-m4a", "audio/m4a"
    };

        public OpenAIService(HttpClient client, IConfiguration configuration)
        {
            _client = client;
            _apiKey = configuration["OpenAI:ApiKey"];
        }

        public async Task<string> TranscribeAsync(
            IFormFile[] files,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new InvalidOperationException(
                    "OpenAI API key is missing. Configure OpenAI:ApiKey through user secrets or environment variables.");
            }

            var sb = new StringBuilder();
            foreach (var file in files)
            {
                var text = string.Empty;
                if (audioTypes.Any(type => type == file.ContentType))
                    text = await TranscribeAudioFileAsync(file, cancellationToken);

                sb.AppendLine(text.Trim());
            }

            return sb.ToString();
        }

        #region  OpenAIIntegrationMethods

        #region Audio
        private async Task<string> TranscribeAudioFileAsync(IFormFile file, CancellationToken ct)
        {
            using var content = new MultipartFormDataContent();
            await using var stream = file.OpenReadStream();

            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            content.Add(fileContent, "file", file.FileName);
            content.Add(new StringContent("whisper-1"), "model");
            content.Add(new StringContent("text"), "response_format");

            using var request = new HttpRequestMessage(
                HttpMethod.Post, "https://api.openai.com/v1/audio/transcriptions")
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            using var response = await _client.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException(
                    $"Transcription failed ({(int)response.StatusCode}): {error}");
            }

            return await response.Content.ReadAsStringAsync(ct);
        }
        #endregion

        #region Completions
        #endregion
        public async Task<AudioAnalysis> AskingForDirections(
            string transcription,
            CancellationToken cancellationToken = default)
        {
            var prompt = new OpenAiRequestPromptModel
            {
                Transcription = transcription
            };
            var requestBody = JsonSerializer.Serialize(new
            {
                model = "gpt-4.1-mini",
                messages = new object[]
                {
                    new { role = "system", content = prompt.BuildSystemPrompt() },
                    new { role = "user", content = prompt.BuildUserPrompt() }
                },
                response_format = new
                {
                    type = "json_schema",
                    json_schema = new
                    {
                        name = "audio_analysis",
                        strict = true,
                        schema = JsonDocument.Parse(AudioAnalysisSchema.Json).RootElement
                    }
                }
            });

            using var request = new HttpRequestMessage(
                HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
            {
                Content = new StringContent(requestBody, Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            using var response = await _client.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(
                await response.Content.ReadAsStringAsync(cancellationToken));
            var answer = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(answer))
                throw new JsonException("OpenAI returned an empty audio analysis.");

            return JsonSerializer.Deserialize<AudioAnalysis>(
                answer,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new JsonException("OpenAI returned an invalid audio analysis.");
        }
        #endregion
    }
}