using MyPersonalFitness.Core.Interfaces;
using MyPersonalFitness.Core.Models;

namespace MyPersonalFitness.Core.Services;

/// <summary>
/// Estimates how long it will take a user to reach their fitness goal based on
/// their starting point, recent progress (calorie deficit/surplus, weight trend),
/// and target goal.
/// </summary>
public class GoalEstimatorService(
    IFitnessGoalRepository goalRepo,
    IUserProfileRepository userRepo,
    IBodyMetricRepository bodyMetricRepo,
    IMealRepository mealRepo)
{
    // One kilogram of body fat ≈ 7,700 kcal
    private const double KcalPerKg = 7700.0;

    /// <summary>
    /// Produces a <see cref="GoalEstimate"/> for the user's active goal.
    /// Returns <c>null</c> if there is insufficient data to produce an estimate.
    /// </summary>
    public async Task<GoalEstimate?> EstimateAsync(int userId)
    {
        var profile = await userRepo.GetCurrentUserAsync();
        var goal = await goalRepo.GetActiveGoalByUserAsync(userId);
        var latestMetric = await bodyMetricRepo.GetLatestByUserAsync(userId);

        if (profile == null || goal == null || latestMetric == null)
            return null;

        // Only produce an estimate for weight-based goals
        if (goal.GoalType is not (GoalType.LoseWeight or GoalType.GainMuscle or GoalType.Maintain))
            return null;

        double currentWeight = latestMetric.WeightKg;
        double targetWeight = goal.TargetWeightKg ?? currentWeight;
        double weightDelta = targetWeight - currentWeight; // negative = need to lose

        // Already at target
        if (Math.Abs(weightDelta) < 0.1)
        {
            return new GoalEstimate
            {
                UserId = userId,
                CurrentWeightKg = currentWeight,
                TargetWeightKg = targetWeight,
                RemainingKg = 0,
                EstimatedWeeksRemaining = 0,
                EstimatedCompletionDate = DateTime.UtcNow.Date,
                WeeklyRateKg = 0,
                DailyCalorieAdjustment = 0,
                ConfidenceLevel = ConfidenceLevel.High
            };
        }

        // --- Determine weekly rate from real progress data ---
        double weeklyRateKg = await GetWeeklyRateFromProgressAsync(userId, weightDelta > 0);

        // If we have no trend data, fall back to calorie-based estimate
        if (weeklyRateKg == 0)
            weeklyRateKg = await EstimateWeeklyRateFromCaloriesAsync(userId, goal, weightDelta > 0);

        if (weeklyRateKg == 0)
            return null; // Cannot estimate without any data

        double weeksRemaining = Math.Abs(weightDelta) / Math.Abs(weeklyRateKg);
        var completionDate = DateTime.UtcNow.Date.AddDays(weeksRemaining * 7);

        // Daily calorie adjustment to stay on track
        double dailyCalorieAdj = weeklyRateKg * KcalPerKg / 7.0;
        if (weightDelta < 0) dailyCalorieAdj = -Math.Abs(dailyCalorieAdj);

        var confidence = DetermineConfidence(userId, latestMetric);

        return new GoalEstimate
        {
            UserId = userId,
            CurrentWeightKg = currentWeight,
            TargetWeightKg = targetWeight,
            RemainingKg = Math.Round(weightDelta, 2),
            EstimatedWeeksRemaining = Math.Round(weeksRemaining, 1),
            EstimatedCompletionDate = completionDate,
            WeeklyRateKg = Math.Round(weeklyRateKg, 2),
            DailyCalorieAdjustment = Math.Round(dailyCalorieAdj, 0),
            ConfidenceLevel = confidence,
            TargetDate = goal.TargetDate,
            IsOnTrack = goal.TargetDate == null
                || completionDate <= goal.TargetDate.Value.AddDays(7)
        };
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private async Task<double> GetWeeklyRateFromProgressAsync(int userId, bool gaining)
    {
        var from = DateTime.UtcNow.AddDays(-56); // last 8 weeks
        var metrics = (await bodyMetricRepo.GetByUserAndDateRangeAsync(userId, from, DateTime.UtcNow))
            .OrderBy(m => m.MeasuredAt)
            .ToList();

        if (metrics.Count < 2)
            return 0;

        var first = metrics.First();
        var last = metrics.Last();
        double weeks = (last.MeasuredAt - first.MeasuredAt).TotalDays / 7.0;
        if (weeks < 0.5) return 0;

        double rate = (last.WeightKg - first.WeightKg) / weeks;

        // Sanity check: rate must agree with goal direction
        if (gaining && rate < 0) return 0;
        if (!gaining && rate > 0) return 0;

        return rate;
    }

    private async Task<double> EstimateWeeklyRateFromCaloriesAsync(
        int userId, FitnessGoal goal, bool gaining)
    {
        // Estimate TDEE from profile
        var profile = await userRepo.GetCurrentUserAsync();
        if (profile == null) return 0;

        var latestMetric = await bodyMetricRepo.GetLatestByUserAsync(userId);
        double weightKg = latestMetric?.WeightKg ?? profile.StartingWeightKg;
        int age = DateTime.UtcNow.Year - profile.DateOfBirth.Year;
        if (DateTime.UtcNow < profile.DateOfBirth.AddYears(age)) age--;

        double bmr = profile.Gender == Gender.Female
            ? 10 * weightKg + 6.25 * profile.HeightCm - 5 * age - 161
            : 10 * weightKg + 6.25 * profile.HeightCm - 5 * age + 5;

        double activityMultiplier = profile.ActivityLevel switch
        {
            ActivityLevel.Sedentary        => 1.2,
            ActivityLevel.LightlyActive    => 1.375,
            ActivityLevel.ModeratelyActive => 1.55,
            ActivityLevel.VeryActive       => 1.725,
            ActivityLevel.ExtraActive      => 1.9,
            _                              => 1.2
        };

        double tdee = bmr * activityMultiplier;

        // Prefer actual average calorie intake from logged meals over the goal target
        double dailyCaloriesIn = await GetAverageDailyCaloriesAsync(userId);
        if (dailyCaloriesIn <= 0 && goal.DailyCalorieTarget.HasValue)
            dailyCaloriesIn = goal.DailyCalorieTarget.Value;

        if (dailyCaloriesIn <= 0) return 0;

        double dailyDelta = dailyCaloriesIn - tdee;
        double weeklyRateKg = dailyDelta * 7 / KcalPerKg;

        // Sanity: cap at reasonable max rates (−1 kg/week cut, +0.5 kg/week bulk)
        weeklyRateKg = gaining
            ? Math.Min(weeklyRateKg, 0.5)
            : Math.Max(weeklyRateKg, -1.0);

        return weeklyRateKg;
    }

    private async Task<double> GetAverageDailyCaloriesAsync(int userId)
    {
        var from = DateTime.UtcNow.AddDays(-30);
        var meals = (await mealRepo.GetByUserAndDateRangeAsync(userId, from, DateTime.UtcNow))
            .ToList();
        if (!meals.Any()) return 0;

        var days = (DateTime.UtcNow.Date - from.Date).Days + 1;
        return meals.Sum(m => m.TotalCalories) / days;
    }

    private static ConfidenceLevel DetermineConfidence(int userId, BodyMetric latestMetric)
    {
        var daysSinceLastMeasurement = (DateTime.UtcNow - latestMetric.MeasuredAt).TotalDays;
        return daysSinceLastMeasurement switch
        {
            <= 3 => ConfidenceLevel.High,
            <= 14 => ConfidenceLevel.Medium,
            _ => ConfidenceLevel.Low
        };
    }
}

/// <summary>
/// Result of a goal timeline estimation.
/// </summary>
public class GoalEstimate
{
    public int UserId { get; init; }
    public double CurrentWeightKg { get; init; }
    public double TargetWeightKg { get; init; }

    /// <summary>How many kilograms remain to reach the target (negative = need to lose).</summary>
    public double RemainingKg { get; init; }

    public double EstimatedWeeksRemaining { get; init; }
    public DateTime EstimatedCompletionDate { get; init; }

    /// <summary>Current rate of weight change in kg/week (positive = gaining, negative = losing).</summary>
    public double WeeklyRateKg { get; init; }

    /// <summary>Daily calorie adjustment required to stay on track.</summary>
    public double DailyCalorieAdjustment { get; init; }

    public ConfidenceLevel ConfidenceLevel { get; init; }
    public DateTime? TargetDate { get; init; }
    public bool IsOnTrack { get; init; }
}

public enum ConfidenceLevel
{
    Low,
    Medium,
    High
}
