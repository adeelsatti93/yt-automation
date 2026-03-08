namespace KidsCartoonPipeline.Core.DTOs.Requests;

public class CreateEpisodeRequest
{
    public int TopicSeedId { get; set; }
    public List<int>? CharacterIds { get; set; }
}
