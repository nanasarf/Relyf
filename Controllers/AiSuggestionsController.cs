using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Relyf.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiSuggestionsController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AiSuggestionsController> _logger;
        private readonly string _apiKey;

        public AiSuggestionsController(IConfiguration config, ILogger<AiSuggestionsController> logger)
        {
            _logger = logger;
            _apiKey = config["Cohere:ApiKey"];

            if (string.IsNullOrEmpty(_apiKey))
                throw new Exception("Cohere API key not found. Configure it in User Secrets or appsettings.json.");

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        [HttpGet("suggest")]
        public async Task<IActionResult> GetSuggestions([FromQuery] string item)
        {
            if (string.IsNullOrWhiteSpace(item))
                return BadRequest(new { error = "Please provide an item name." });

            try
            {
                _logger.LogInformation("Sending Cohere request for item: {Item}", item);

                // Build request body
                var requestBody = new
                {
                    model = "command",
                    prompt = $"List 3 creative, safe ways to reuse or recycle: {item}. Format as a numbered list.",
                    max_tokens = 100,
                    temperature = 0.7
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Send request to Cohere API
                var response = await _httpClient.PostAsync("https://api.cohere.ai/v1/generate", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Cohere API Error: {Response}", responseString);
                    return StatusCode((int)response.StatusCode, new { error = "Failed to get suggestions from Cohere." });
                }

                using var doc = JsonDocument.Parse(responseString);
                var suggestions = doc.RootElement
                    .GetProperty("generations")[0]
                    .GetProperty("text")
                    .GetString()
                    ?.Trim();

                if (string.IsNullOrWhiteSpace(suggestions))
                {
                    _logger.LogWarning("Cohere returned empty text for {Item}", item);
                    suggestions = "No suggestions available at the moment. Please try again later.";
                }

                _logger.LogInformation("Cohere responded with: {Response}", suggestions);

                return Ok(new { item, suggestions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting Cohere suggestions for {Item}", item);
                return StatusCode(500, new
                {
                    error = "An error occurred while processing your request. Please try again later."
                });
            }
        }
    }
}
