using System.ClientModel;
using System.Text.Json;
using Azure.AI.OpenAI;
using GreenLens.Core.Interfaces;
using GreenLens.Core.Models;
using GreenLens.Shared.Constants;
using GreenLens.Shared.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace GreenLens.Infrastructure.Services;

/// <summary>
/// Azure OpenAI implementation of the recommendation service.
/// Generates AI-powered carbon reduction recommendations for completed estimates.
/// Includes in-memory caching to avoid redundant API calls.
/// </summary>
public class AzureRecommendationService : IRecommendationService
{
    private readonly ChatClient _chatClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AzureRecommendationService> _logger;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    public AzureRecommendationService(
        IConfiguration configuration,
        IMemoryCache cache,
        ILogger<AzureRecommendationService> logger)
    {
        _cache = cache;
        _logger = logger;

        var endpoint = configuration["AZURE_OPENAI_ENDPOINT"]
            ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not configured.");
        var apiKey = configuration["AZURE_OPENAI_API_KEY"]
            ?? throw new InvalidOperationException("AZURE_OPENAI_API_KEY is not configured.");
        var deploymentName = configuration["AZURE_OPENAI_DEPLOYMENT_NAME"] ?? "gpt-4o-mini";

        var client = new AzureOpenAIClient(
            new Uri(endpoint),
            new ApiKeyCredential(apiKey));
        _chatClient = client.GetChatClient(deploymentName);
    }

    /// <inheritdoc />
    public async Task<List<RecommendationResponse>> GenerateAsync(
        CarbonEstimate estimate,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"recommendations:{estimate.Id}";

        if (_cache.TryGetValue(cacheKey, out List<RecommendationResponse>? cached) && cached is not null)
        {
            _logger.LogDebug("Returning cached recommendations for estimate {EstimateId}", estimate.Id);
            return cached;
        }

        try
        {
            var prompt = BuildPrompt(estimate);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(
                    "You are a cloud sustainability expert. Analyze the given Azure cloud resource usage " +
                    "and carbon footprint data. Respond with exactly 3-5 actionable recommendations to reduce " +
                    "carbon emissions. Respond ONLY with a valid JSON array. Each object in the array must have: " +
                    "\"title\" (short action title), \"description\" (1-2 sentence explanation), " +
                    "\"estimatedReductionPercent\" (number 1-100), \"effort\" (exactly one of: \"Low\", \"Medium\", \"High\"). " +
                    "No markdown, no code fences, just the raw JSON array."),
                new UserChatMessage(prompt)
            };

            var options = new ChatCompletionOptions
            {
                Temperature = 0.3f,
                MaxOutputTokenCount = 1024
            };

            var completion = await _chatClient.CompleteChatAsync(messages, options, cancellationToken);
            var responseText = completion.Value.Content[0].Text;

            var recommendations = ParseRecommendations(responseText);

            _cache.Set(cacheKey, recommendations, CacheDuration);

            return recommendations;
        }
        catch (ClientResultException ex) when (ex.Status == 429)
        {
            _logger.LogWarning("Azure OpenAI rate limited: {Message}", ex.Message);
            throw new AppException(
                ErrorCodes.ServiceUnavailable,
                "AI recommendation service is temporarily rate limited. Please try again later.",
                503);
        }
        catch (ClientResultException ex)
        {
            _logger.LogError(ex, "Azure OpenAI request failed with status {Status}", ex.Status);
            throw new AppException(
                ErrorCodes.ServiceUnavailable,
                "AI recommendation service is currently unavailable. Please try again later.",
                503);
        }
    }

    /// <summary>
    /// Builds the user prompt from the estimate data.
    /// </summary>
    private static string BuildPrompt(CarbonEstimate estimate)
    {
        var resourceLines = estimate.Resources.Select(r =>
            $"- {r.Quantity}x {r.ResourceType} in {r.Region}: {r.Hours} hours, {r.Co2eKg:F2} kg CO2e ({r.Co2ePerUnit} {r.Unit})");

        return $"""
            Azure cloud infrastructure carbon footprint analysis:

            Total CO2e: {estimate.TotalCo2eKg:F2} kg
            Resources:
            {string.Join("\n", resourceLines)}

            Provide 3-5 specific, actionable recommendations to reduce the carbon footprint of this infrastructure.
            Consider: VM right-sizing, spot/burstable instances, region selection, reserved capacity, storage optimization.
            """;
    }

    /// <summary>
    /// Parses the JSON response from Azure OpenAI into typed recommendations.
    /// </summary>
    private List<RecommendationResponse> ParseRecommendations(string responseText)
    {
        try
        {
            // Strip markdown code fences if present
            var cleaned = responseText.Trim();
            if (cleaned.StartsWith("```"))
            {
                var firstNewline = cleaned.IndexOf('\n');
                if (firstNewline > 0)
                    cleaned = cleaned[(firstNewline + 1)..];
                if (cleaned.EndsWith("```"))
                    cleaned = cleaned[..^3].Trim();
            }

            var recommendations = JsonSerializer.Deserialize<List<RecommendationResponse>>(
                cleaned,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (recommendations is null || recommendations.Count == 0)
            {
                _logger.LogWarning("Azure OpenAI returned empty or unparseable recommendations");
                return GetFallbackRecommendations();
            }

            // Validate and clamp values
            return recommendations.Select(r => new RecommendationResponse
            {
                Title = string.IsNullOrWhiteSpace(r.Title) ? "Optimize resource usage" : r.Title,
                Description = string.IsNullOrWhiteSpace(r.Description) ? "Consider optimizing your cloud resources." : r.Description,
                EstimatedReductionPercent = Math.Clamp(r.EstimatedReductionPercent, 0, 100),
                Effort = ValidateEffort(r.Effort)
            }).ToList();
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse Azure OpenAI response: {Response}", responseText);
            return GetFallbackRecommendations();
        }
    }

    private static string ValidateEffort(string effort)
    {
        return effort switch
        {
            "Low" or "Medium" or "High" => effort,
            _ => "Medium"
        };
    }

    private static List<RecommendationResponse> GetFallbackRecommendations()
    {
        return new List<RecommendationResponse>
        {
            new()
            {
                Title = "Consider B-series burstable VMs",
                Description = "B-series VMs use less energy during idle periods, reducing carbon emissions for variable workloads.",
                EstimatedReductionPercent = 30,
                Effort = "Low"
            },
            new()
            {
                Title = "Move workloads to greener regions",
                Description = "Regions like Sweden Central and France Central have lower grid carbon intensity than other Azure regions.",
                EstimatedReductionPercent = 25,
                Effort = "Medium"
            },
            new()
            {
                Title = "Right-size over-provisioned resources",
                Description = "Analyze CPU and memory utilization to identify VMs that can be downsized without impacting performance.",
                EstimatedReductionPercent = 20,
                Effort = "Medium"
            }
        };
    }
}
