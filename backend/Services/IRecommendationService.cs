using ClimateAdvisor.Api.Models;

namespace ClimateAdvisor.Api.Services;

public interface IRecommendationService
{
    Task<RecommendationResponse> GenerateRecommendationsAsync(
        double latitude, double longitude, CancellationToken ct = default);
}
