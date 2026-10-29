using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using OpenAISrvc.Models;
using OpenAISrvc.Schemas;

namespace OpenAISrvc
{  
    public sealed class OpenAIService
    {
        private const string TranscriptionUrl = "https://api.openai.com/v1/audio/transcriptions";
        private const string ChatCompletionsUrl = "https://api.openai.com/v1/chat/completions";

        private readonly HttpClient _client;
        private readonly ILogger<OpenAIService> _logger;
        private readonly string? _apiKey;
        private readonly string _transcriptionModel;
        private readonly string _analysisModel;
        private readonly OpenAILimits _limits;

        private static readonly HashSet<string> AudioTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "audio/mpeg", "audio/mp3", "audio/wav",
            "audio/x-wav", "audio/wave", "audio/mp4",
            "audio/x-m4a", "audio/m4a"
        };

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public OpenAIService(
            HttpClient client,
            IConfiguration configuration,
            ILogger<OpenAIService> logger)
        {
            _client = client;
            _logger = logger;
            _apiKey = configuration["OpenAI:ApiKey"];
            _transcriptionModel = configuration["OpenAI:TranscriptionModel"] ?? "gpt-4o-mini-transcribe";
            _analysisModel = configuration["OpenAI:AnalysisModel"] ?? "gpt-4.1-mini";
            _limits = configuration.GetSection("OpenAI:Limits").Get<OpenAILimits>() ?? new OpenAILimits();
        }

        public async Task<string> TranscribeAsync(
            IFormFile[] files,
            CancellationToken cancellationToken = default)
        {
            EnsureApiKey();
            ValidateFiles(files);

            var sb = new StringBuilder();
            foreach (var file in files)
            {
                var text = await TranscribeAudioFileAsync(file, cancellationToken);
                sb.AppendLine(text.Trim());
            }

            return sb.ToString();
        }

        public async Task<AudioAnalysis> AskingForDirections(
            string transcription,
            CancellationToken cancellationToken = default)
        {
            EnsureApiKey();

            if (string.IsNullOrWhiteSpace(transcription))
                throw new ArgumentException("Transcription is empty.", nameof(transcription));
            
            var estimatedTokens = transcription.Length / 4;
            if (estimatedTokens > _limits.MaxTranscriptTokens)
            {
                throw new ArgumentException(
                    $"Transcript is too long for one analysis call (about {estimatedTokens:N0} tokens, " +
                    $"limit {_limits.MaxTranscriptTokens:N0}).");
            }

            var prompt = new OpenAiRequestPromptModel
            {
                Transcription = transcription
            };

            var requestBody = JsonSerializer.Serialize(new
            {
                model = _analysisModel,
                max_completion_tokens = _limits.MaxOutputTokens,
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

            using var request = CreateRequest(
                ChatCompletionsUrl,
                new StringContent(requestBody, Encoding.UTF8, "application/json"));

            using var response = await _client.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, "Analysis", cancellationToken);

            using var doc = JsonDocument.Parse(
                await response.Content.ReadAsStringAsync(cancellationToken));

            LogUsage(doc.RootElement);

            var choice = doc.RootElement.GetProperty("choices")[0];
            if (choice.TryGetProperty("finish_reason", out var finishReason) &&
                finishReason.GetString() == "length")
            {
                throw new JsonException(
                    "The analysis was cut off by the output limit. Raise OpenAI:Limits:MaxOutputTokens.");
            }

            var message = choice.GetProperty("message");
            if (message.TryGetProperty("refusal", out var refusal) &&
                refusal.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(refusal.GetString()))
            {
                throw new InvalidOperationException($"OpenAI refused the request: {refusal.GetString()}");
            }

            var answer = message.GetProperty("content").GetString();
            if (string.IsNullOrWhiteSpace(answer))
                throw new JsonException("OpenAI returned an empty audio analysis.");

            return JsonSerializer.Deserialize<AudioAnalysis>(answer, JsonOptions)
                ?? throw new JsonException("OpenAI returned an invalid audio analysis.");
        }

        #region Validation

        private void EnsureApiKey()
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new InvalidOperationException(
                    "OpenAI API key is missing. Configure OpenAI:ApiKey through user secrets or environment variables.");
            }
        }

        private void ValidateFiles(IFormFile[] files)
        {
            if (files is null || files.Length == 0 || files.Length > _limits.MaxFiles)
                throw new ArgumentException($"Send between 1 and {_limits.MaxFiles} files.");

            foreach (var file in files)
            {
                if (!AudioTypes.Contains(file.ContentType))
                {
                    throw new ArgumentException(
                        $"Unsupported file type for '{file.FileName}': {file.ContentType}. " +
                        "Extract the audio from videos before uploading.");
                }

                if (file.Length == 0)
                    throw new ArgumentException($"'{file.FileName}' is empty.");

                if (file.Length > _limits.MaxFileBytes)
                {
                    throw new ArgumentException(
                        $"'{file.FileName}' exceeds {_limits.MaxFileBytes / 1024 / 1024} MB. " +
                        "Split the audio into smaller chunks.");
                }
            }
        }

        #endregion

        #region OpenAI integration methods

        private async Task<string> TranscribeAudioFileAsync(IFormFile file, CancellationToken ct)
        {
            using var content = new MultipartFormDataContent();
            await using var stream = file.OpenReadStream();

            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

            content.Add(fileContent, "file", file.FileName);
            content.Add(new StringContent(_transcriptionModel), "model");
            content.Add(new StringContent("json"), "response_format");

            using var request = CreateRequest(TranscriptionUrl, content);
            using var response = await _client.SendAsync(request, ct);
            await EnsureSuccessAsync(response, "Transcription", ct);

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
            var text = doc.RootElement.GetProperty("text").GetString() ?? string.Empty;

            _logger.LogInformation(
                "Transcribed '{FileName}' ({SizeKb:N0} KB) with {Model}: {Chars:N0} characters.",
                file.FileName, file.Length / 1024, _transcriptionModel, text.Length);

            return text;
        }

        private HttpRequestMessage CreateRequest(string url, HttpContent content)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            return request;
        }

        private static async Task EnsureSuccessAsync(
            HttpResponseMessage response,
            string operation,
            CancellationToken ct)
        {
            if (response.IsSuccessStatusCode)
                return;

            var error = await response.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"{operation} failed ({(int)response.StatusCode}): {error}");
        }

        private void LogUsage(JsonElement root)
        {
            if (!root.TryGetProperty("usage", out var usage))
                return;

            _logger.LogInformation(
                "OpenAI analysis usage with {Model}: {PromptTokens} input tokens, {CompletionTokens} output tokens.",
                _analysisModel,
                usage.GetProperty("prompt_tokens").GetInt32(),
                usage.GetProperty("completion_tokens").GetInt32());
        }

        #endregion
    }
}