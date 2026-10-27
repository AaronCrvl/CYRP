using System.Text.Json;
namespace OpenAiSerivce.Models
{
    public class OpenAiRequestPromptModel
    {
        public string transcriptions { get; set; } = string.Empty;
        public string actions { get; set; } = string.Empty;
        public string filesToGenerate { get; set; } = string.Empty;

        public OpenAiRequestPromptModel(string? customActions, string? customFiles)
        {
            if (customActions != null && string.IsNullOrEmpty(customActions))
                this.actions = JsonSerializer.Serialize(customActions);
            else
                SetBaseActions();

            if (customFiles != null && string.IsNullOrEmpty(customFiles))
                this.filesToGenerate = JsonSerializer.Serialize(customFiles);
            else
                SetBaseFilesToGenerate();
        }

        public void SetBaseActions()
        {
            this.actions = JsonSerializer.Serialize(new
            {
                action1 = "plan ...",
                action2 = "create ...",
                action3 = "create ..."
            });
        }

        public void SetBaseFilesToGenerate()
        {
            this.filesToGenerate = JsonSerializer.Serialize(new
            {
                file1 = "generate ...",
                file2 = "create ...",
                file3 = "create ..."
            });
        }
    }
}