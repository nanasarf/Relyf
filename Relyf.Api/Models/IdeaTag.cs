namespace Relyf.Api.Models;

public class IdeaTag
{
    public int IdeaId { get; set; }  // PK part -> app.AiIdea
    public int TagId { get; set; }   // PK part -> app.Tag
}
