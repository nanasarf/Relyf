namespace Relyf.Api.Models;

public class Item
{
    public int ItemId { get; set; }           
    public int UserId { get; set; }               
    public string Title { get; set; } = "";
    public string? Description { get; set; }
}
