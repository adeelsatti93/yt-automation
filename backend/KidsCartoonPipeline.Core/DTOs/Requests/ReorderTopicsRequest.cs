namespace KidsCartoonPipeline.Core.DTOs.Requests;

public class ReorderTopicsRequest
{
    public List<TopicPriorityItem> Items { get; set; } = [];
}

public class TopicPriorityItem
{
    public int Id { get; set; }
    public int Priority { get; set; }
}
