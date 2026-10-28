namespace OpenAISrvc.Schemas.Records
{
public sealed record ActionItem(string Title, string Description, string Column,
      string Priority, string? Owner, string? DueDate, string? Timestamp);
}