using Microsoft.AspNetCore.Http;

namespace Relyf.Api.Controllers.Models;

public sealed class ImageUploadRequest
{
    public IFormFile? File { get; set; }
    public string? Folder { get; set; }
}