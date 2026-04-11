using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace PersonalProjectNotes.Server.Controllers;

[ApiController]
[Route("api/ai")]
public sealed class AiController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AiController> _logger;

    public AiController(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AiController> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> AskGemini(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Prompt))
        {
            return BadRequest(new ErrorResponse("Промпт порожній."));
        }

        var apiKey = _configuration["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogError("Gemini API key is missing in configuration.");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ErrorResponse("Gemini API key не налаштований."));
        }

        var model = _configuration["Gemini:Model"] ?? "gemini-2.5-flash";
        var endpoint =
            $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        var payload = new GeminiGenerateContentRequest(
            [
                new GeminiContent(
                    [
                        new GeminiPart(request.Prompt)
                    ])
            ]);

        try
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = JsonContent.Create(payload)
            };

            httpRequest.Headers.Accept.Clear();
            httpRequest.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            var rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Gemini API returned error. Status: {StatusCode}. Body: {Body}",
                    (int)response.StatusCode,
                    rawResponse);

                return StatusCode((int)response.StatusCode, new
                {
                    error = "Google API Error",
                    details = rawResponse
                });
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var result = JsonSerializer.Deserialize<GeminiGenerateContentResponse>(rawResponse, options);
            var aiText = result?.Candidates?.FirstOrDefault()
                ?.Content?.Parts?.FirstOrDefault()
                ?.Text;

            if (string.IsNullOrWhiteSpace(aiText))
            {
                _logger.LogWarning("Gemini response was successful but text was empty. Body: {Body}", rawResponse);
                return Ok(new ChatResponse("Empty response"));
            }

            return Ok(new ChatResponse(aiText));
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Request to Gemini was cancelled.");
            return StatusCode(StatusCodes.Status499ClientClosedRequest,
                new ErrorResponse("Запит було скасовано."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while calling Gemini API.");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ErrorResponse("Внутрішня помилка сервера."));
        }
    }
}

public sealed record ChatRequest(
    [property: JsonPropertyName("prompt")] string Prompt);

public sealed record ChatResponse(
    [property: JsonPropertyName("text")] string Text);

public sealed record ErrorResponse(
    [property: JsonPropertyName("error")] string Error);

public sealed record GeminiGenerateContentRequest(
    [property: JsonPropertyName("contents")] List<GeminiContent> Contents);

public sealed record GeminiContent(
    [property: JsonPropertyName("parts")] List<GeminiPart> Parts);

public sealed record GeminiPart(
    [property: JsonPropertyName("text")] string Text);

public sealed record GeminiGenerateContentResponse(
    [property: JsonPropertyName("candidates")] List<GeminiCandidate>? Candidates);

public sealed record GeminiCandidate(
    [property: JsonPropertyName("content")] GeminiContent? Content);
