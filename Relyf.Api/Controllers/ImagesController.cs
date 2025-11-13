using Microsoft.AspNetCore.Mvc;
using Relyf.Repository.Dapper;

namespace Relyf.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ImagesController : ControllerBase
{
    private static readonly HashSet<string> AllowedOwners = new(StringComparer.OrdinalIgnoreCase) { "Item", "Idea", "Project" };
    private static readonly HashSet<string> AllowedSources = new(StringComparer.OrdinalIgnoreCase) { "upload", "url", "cloudinary" };
    private static readonly HashSet<string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase) 
    { 
        "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" 
    };
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

    private readonly IImageRepository _repo;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ImagesController> _logger;

    public ImagesController(
        IImageRepository repo, 
        IWebHostEnvironment env,
        ILogger<ImagesController> logger)
    {
        _repo = repo;
        _env = env;
        _logger = logger;
    }

    /// <summary>
    /// Upload image request (JSON-based, for backward compatibility)
    /// </summary>
    public sealed record AddImageRequest(
        string OwnerType, 
        int OwnerId, 
        string? Source = null,      // Optional: base64 data, external URL, or leave empty for file upload
        string? Url = null,          // Optional: external URL or will be generated for uploads
        string? AltText = null);

    /// <summary>
    /// File upload request model for multipart/form-data
    /// </summary>
    public sealed class FileUploadRequest
    {
        public IFormFile File { get; set; } = null!;
        public string OwnerType { get; set; } = null!;
        public int OwnerId { get; set; }
        public string? AltText { get; set; }
    }

    /// <summary>
    /// POST /api/images - Upload image via JSON (backward compatible)
    /// Supports three modes:
    /// 1. External URL: { "ownerType": "Project", "ownerId": 1, "source": "url", "url": "https://..." }
    /// 2. Base64 data: { "ownerType": "Project", "ownerId": 1, "source": "data:image/jpeg;base64,..." }
    /// 3. Cloudinary: { "ownerType": "Project", "ownerId": 1, "source": "cloudinary", "url": "https://res.cloudinary.com/..." }
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    public async Task<IActionResult> Add([FromBody] AddImageRequest req, CancellationToken ct)
    {
        if (!AllowedOwners.Contains(req.OwnerType)) 
            return BadRequest(new { error = "OwnerType must be Item, Idea, or Project." });

        if (!await _repo.OwnerExistsAsync(req.OwnerType, req.OwnerId))
            return BadRequest(new { error = "Owner not found." });

        string source;
        string url;

        // Handle different upload modes
        if (!string.IsNullOrWhiteSpace(req.Source))
        {
            // Mode 1: Base64 data URL (e.g., "data:image/jpeg;base64,...")
            if (req.Source.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var savedPath = await SaveBase64ImageAsync(req.Source);
                    source = "upload";
                    url = savedPath;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save base64 image");
                    return BadRequest(new { error = "Invalid base64 image data." });
                }
            }
            // Mode 2: External URL or Cloudinary
            else if (!string.IsNullOrWhiteSpace(req.Url))
            {
                if (!AllowedSources.Contains(req.Source))
                    return BadRequest(new { error = "Source must be upload, url, or cloudinary." });
                
                source = req.Source;
                url = req.Url;
            }
            else
            {
                return BadRequest(new { error = "When Source is provided without data URL, Url is required." });
            }
        }
        else if (!string.IsNullOrWhiteSpace(req.Url))
        {
            // Legacy mode: just URL provided
            source = "url";
            url = req.Url;
        }
        else
        {
            return BadRequest(new { error = "Either Source (with data URL) or Url must be provided." });
        }

        var id = await _repo.AddAsync(req.OwnerType, req.OwnerId, source, url, req.AltText);
        return CreatedAtAction(nameof(List), new { ownerType = req.OwnerType, ownerId = req.OwnerId }, 
            new { imageId = id, url });
    }

    /// <summary>
    /// POST /api/images/upload - Upload image via multipart/form-data (recommended)
    /// Usage: 
    /// - file: binary file data
    /// - ownerType: "Item" | "Idea" | "Project"
    /// - ownerId: integer
    /// - altText: optional string
    /// </summary>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadFile([FromForm] FileUploadRequest request, CancellationToken ct = default)
    {
        // Validate inputs
        if (request.File == null || request.File.Length == 0)
            return BadRequest(new { error = "File is required." });

        if (!AllowedOwners.Contains(request.OwnerType))
            return BadRequest(new { error = "OwnerType must be Item, Idea, or Project." });

        if (!AllowedImageTypes.Contains(request.File.ContentType))
            return BadRequest(new { error = $"Invalid file type. Allowed types: {string.Join(", ", AllowedImageTypes)}" });

        if (request.File.Length > MaxFileSizeBytes)
            return BadRequest(new { error = $"File size exceeds maximum of {MaxFileSizeBytes / 1024 / 1024}MB." });

        if (!await _repo.OwnerExistsAsync(request.OwnerType, request.OwnerId))
            return BadRequest(new { error = "Owner not found." });

        try
        {
            // Save file to disk
            var savedPath = await SaveUploadedFileAsync(request.File);

            // Store in database
            var id = await _repo.AddAsync(request.OwnerType, request.OwnerId, "upload", savedPath, request.AltText);

            return CreatedAtAction(nameof(List), new { ownerType = request.OwnerType, ownerId = request.OwnerId },
                new { imageId = id, url = savedPath, fileName = request.File.FileName });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload image file");
            return StatusCode(500, new { error = "Failed to save image." });
        }
    }

    // GET /api/images/{ownerType}/{ownerId}
    [HttpGet("{ownerType}/{ownerId:int}")]
    public async Task<IActionResult> List(string ownerType, int ownerId, CancellationToken ct)
    {
        if (!AllowedOwners.Contains(ownerType)) 
            return BadRequest(new { error = "Invalid ownerType." });
        
        var list = await _repo.ListByOwnerAsync(ownerType, ownerId);
        return Ok(list);
    }

    // DELETE /api/images/{imageId}
    [HttpDelete("{imageId:int}")]
    public async Task<IActionResult> Delete(int imageId, CancellationToken ct)
    {
        var n = await _repo.DeleteAsync(imageId);
        return n == 0 ? NotFound() : NoContent();
    }

    /// <summary>
    /// Save uploaded file to disk and return relative path
    /// </summary>
    private async Task<string> SaveUploadedFileAsync(IFormFile file)
    {
        // Create uploads directory if it doesn't exist
        var uploadsPath = Path.Combine(_env.ContentRootPath, "uploads", "images");
        Directory.CreateDirectory(uploadsPath);

        // Generate unique filename
        var fileExt = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{fileExt}";
        var filePath = Path.Combine(uploadsPath, fileName);

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Return relative path for storage
        return $"/uploads/images/{fileName}";
    }

    /// <summary>
    /// Save base64 image data to disk and return relative path
    /// </summary>
    private async Task<string> SaveBase64ImageAsync(string dataUrl)
    {
        // Parse data URL: "data:image/jpeg;base64,..."
        var parts = dataUrl.Split(',');
        if (parts.Length != 2)
            throw new ArgumentException("Invalid data URL format");

        var base64Data = parts[1];
        var mimeType = parts[0].Split(':')[1].Split(';')[0]; // Extract "image/jpeg"

        if (!AllowedImageTypes.Contains(mimeType))
            throw new ArgumentException($"Invalid image type: {mimeType}");

        // Decode base64
        var imageBytes = Convert.FromBase64String(base64Data);

        if (imageBytes.Length > MaxFileSizeBytes)
            throw new ArgumentException($"Image size exceeds maximum of {MaxFileSizeBytes / 1024 / 1024}MB");

        // Determine file extension
        var ext = mimeType switch
        {
            "image/jpeg" or "image/jpg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            "image/webp" => ".webp",
            _ => ".jpg"
        };

        // Create uploads directory
        var uploadsPath = Path.Combine(_env.ContentRootPath, "uploads", "images");
        Directory.CreateDirectory(uploadsPath);

        // Generate unique filename
        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsPath, fileName);

        // Save file
        await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

        // Return relative path
        return $"/uploads/images/{fileName}";
    }
}
