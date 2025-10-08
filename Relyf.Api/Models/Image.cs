namespace Relyf.Api.Models;

public class Image
{
    public int ImageId { get; set; }          // PK
    public string OwnerType { get; set; } = "Item";   // 'Item' | 'Idea' | 'Project'
    public int OwnerId { get; set; }                  // FK resolved by OwnerType
    public string Source { get; set; } = "url";       // 'upload' | 'url' | 'cloudinary'
    public string Url { get; set; } = "";             // absolute URL
    public string? AltText { get; set; }
}
