using MyPersonalFitness.Core.Interfaces;
using MyPersonalFitness.Core.Models;

namespace MyPersonalFitness.Core.Services;

/// <summary>
/// Handles body metric tracking and trend analysis.
/// </summary>
public class BodyMetricService(IBodyMetricRepository bodyMetricRepo, IUserProfileRepository userRepo)
{
    public async Task<BodyMetric> LogBodyMetricAsync(BodyMetric metric)
    {
        metric.MeasuredAt = metric.MeasuredAt == default ? DateTime.UtcNow : metric.MeasuredAt;
        await bodyMetricRepo.AddAsync(metric);
        return metric;
    }

    public Task<IEnumerable<BodyMetric>> GetHistoryAsync(int userId) =>
        bodyMetricRepo.GetByUserAsync(userId);

    public Task<IEnumerable<BodyMetric>> GetHistoryAsync(int userId, DateTime from, DateTime to) =>
        bodyMetricRepo.GetByUserAndDateRangeAsync(userId, from, to);

    public Task<BodyMetric?> GetLatestAsync(int userId) =>
        bodyMetricRepo.GetLatestByUserAsync(userId);

    /// <summary>
    /// Calculates total weight change since the user's starting measurement.
    /// Negative means weight lost; positive means gained.
    /// </summary>
    public async Task<double?> GetTotalWeightChangeAsync(int userId)
    {
        var profile = await userRepo.GetCurrentUserAsync();
        var latest = await bodyMetricRepo.GetLatestByUserAsync(userId);
        if (profile == null || latest == null) return null;
        return Math.Round(latest.WeightKg - profile.StartingWeightKg, 2);
    }

    /// <summary>
    /// Returns weight change trend over the specified period in kg/week.
    /// </summary>
    public async Task<double?> GetWeightTrendKgPerWeekAsync(int userId, int lookbackDays = 30)
    {
        var from = DateTime.UtcNow.AddDays(-lookbackDays);
        var metrics = (await bodyMetricRepo.GetByUserAndDateRangeAsync(userId, from, DateTime.UtcNow))
            .OrderBy(m => m.MeasuredAt)
            .ToList();

        if (metrics.Count < 2) return null;

        var first = metrics.First();
        var last = metrics.Last();
        var weeks = (last.MeasuredAt - first.MeasuredAt).TotalDays / 7.0;
        if (weeks <= 0) return null;

        return Math.Round((last.WeightKg - first.WeightKg) / weeks, 2);
    }

    /// <summary>
    /// Computes TDEE (Total Daily Energy Expenditure) using the Mifflin-St Jeor equation.
    /// </summary>
    public async Task<double?> CalculateTdeeAsync(int userId)
    {
        var profile = await userRepo.GetCurrentUserAsync();
        var latestMetric = await bodyMetricRepo.GetLatestByUserAsync(userId);
        if (profile == null) return null;

        double weightKg = latestMetric?.WeightKg ?? profile.StartingWeightKg;
        double heightCm = profile.HeightCm;
        int age = DateTime.UtcNow.Year - profile.DateOfBirth.Year;
        if (DateTime.UtcNow < profile.DateOfBirth.AddYears(age)) age--;

        // Mifflin-St Jeor BMR
        double bmr = profile.Gender == Gender.Female
            ? 10 * weightKg + 6.25 * heightCm - 5 * age - 161
            : 10 * weightKg + 6.25 * heightCm - 5 * age + 5;

        double tdee = profile.ActivityLevel switch
        {
            ActivityLevel.Sedentary        => bmr * 1.2,
            ActivityLevel.LightlyActive    => bmr * 1.375,
            ActivityLevel.ModeratelyActive => bmr * 1.55,
            ActivityLevel.VeryActive       => bmr * 1.725,
            ActivityLevel.ExtraActive      => bmr * 1.9,
            _                              => bmr * 1.2
        };

        return Math.Round(tdee, 0);
    }
}
