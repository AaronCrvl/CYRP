namespace OpenAISrvc.Schemas
{
  public static class AudioAnalysisSchema
  {
    public const string Json = """
    {
      "type": "object",
      "additionalProperties": false,
      "required": ["audioType","summary","timeNotes","actions","studyNotes","filesToGenerate"],
      "properties": {
        "audioType": { "type": "string", "enum": ["meeting","class","unrelated"] },
        "summary":   { "type": "string" },
        "timeNotes": {
          "type": "array",
          "items": {
            "type": "object", "additionalProperties": false,
            "required": ["timestamp","topic","note"],
            "properties": {
              "timestamp": { "type": ["string","null"] },
              "topic": { "type": "string" },
              "note":  { "type": "string" }
            }
          }
        },
        "actions": {
          "type": "array",
          "items": {
            "type": "object", "additionalProperties": false,
            "required": ["title","description","column","priority","owner","dueDate","timestamp"],
            "properties": {
              "title": { "type": "string" },
              "description": { "type": "string" },
              "column": { "type": "string", "enum": ["backlog","todo","doing","done"] },
              "priority": { "type": "string", "enum": ["low","medium","high"] },
              "owner": { "type": ["string","null"] },
              "dueDate": { "type": ["string","null"] },
              "timestamp": { "type": ["string","null"] }
            }
          }
        },
        "studyNotes": { "type": "array", "items": { "type": "string" } },
        "filesToGenerate": {
          "type": "array",
          "items": {
            "type": "object", "additionalProperties": false,
            "required": ["fileName","type","description","content"],
            "properties": {
              "fileName": { "type": "string" },
              "type": { "type": "string", "enum": ["mermaid"] },
              "description": { "type": "string" },
              "content": { "type": "string" }
            }
          }
        }
      }
    }
    """;
  }

  public sealed record AudioAnalysis(
      string AudioType, string Summary,
      List<Records.TimeNote> TimeNotes, List<Records.ActionItem> Actions,
      List<string> StudyNotes, List<Records.GeneratedFile> FilesToGenerate);    
}