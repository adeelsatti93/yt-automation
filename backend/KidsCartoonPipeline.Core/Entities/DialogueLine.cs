namespace KidsCartoonPipeline.Core.Entities;

public class DialogueLine
{
    public int Id { get; set; }
    public int SceneId { get; set; }
    public Scene Scene { get; set; } = null!;
    public int LineOrder { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string? Tone { get; set; }
    public string? AudioPath { get; set; }
}
