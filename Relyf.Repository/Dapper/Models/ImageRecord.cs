namespace Relyf.Repository.Dapper.Models;

public sealed class ImageRecord
{
    public int ImageId { get; init; }
    public string OwnerType { get; init; } = ""; // "Item" | "Idea" | "Project"
    public int OwnerId { get; init; }
    public string Source { get; init; } = "";    // "upload" | "url" | "cloudinary"
    public string Url { get; init; } = "";
    public string? AltText { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}
