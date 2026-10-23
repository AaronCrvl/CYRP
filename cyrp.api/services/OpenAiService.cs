using OpenAI.Responses;

public class OpenAIService
{
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    private readonly ResponsesClient _client;
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    private readonly string _model;

    public OpenAIService(IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI::API_LEY"];
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        _client = new ResponsesClient(apiKey);
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
    }

    public async Task<string?> AskSync(string prompt,
        CancellationToken cancellationToken = default)
    {
        var res = await _client.CreateResponseAsync(
              _model,
            prompt,
            cancellationToken: cancellationToken
        );

        return res.GetRawResponse()?.ToString();
    }
}