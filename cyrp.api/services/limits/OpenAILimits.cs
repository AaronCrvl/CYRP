public sealed class OpenAILimits
{
    public long MaxFileBytes { get; init; } = 25 * 1024 * 1024;
    public int MaxFiles { get; init; } = 5;
    public int MaxTranscriptTokens { get; init; } = 30_000;
    public int MaxOutputTokens { get; init; } = 2_000;
}