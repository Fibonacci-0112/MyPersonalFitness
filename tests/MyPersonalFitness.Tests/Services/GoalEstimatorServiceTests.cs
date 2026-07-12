using MyPersonalFitness.Core.Data;
using MyPersonalFitness.Core.Interfaces;
using MyPersonalFitness.Core.Models;
using MyPersonalFitness.Core.Services;

namespace MyPersonalFitness.Tests.Services;

public class GoalEstimatorServiceTests
{
    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static (GoalEstimatorService service,
                    IUserProfileRepository userRepo,
                    IBodyMetricRepository bodyRepo,
                    IFitnessGoalRepository goalRepo,
                    IMealRepository mealRepo) BuildService()
    {
        var userRepo = new InMemoryUserProfileRepository();
        var bodyRepo = new InMemoryBodyMetricRepository();
        var goalRepo = new InMemoryFitnessGoalRepository();
        var mealRepo = new InMemoryMealRepository();

        var svc = new GoalEstimatorService(goalRepo, userRepo, bodyRepo, mealRepo);
        return (svc, userRepo, bodyRepo, goalRepo, mealRepo);
    }

    private static UserProfile MakeProfile(double startWeightKg = 90, double heightCm = 175) =>
        new()
        {
            Name = "Test User",
            DateOfBirth = DateTime.UtcNow.AddYears(-30),
            Gender = Gender.Male,
            HeightCm = heightCm,
            StartingWeightKg = startWeightKg,
            ActivityLevel = ActivityLevel.ModeratelyActive,
            StartDate = DateTime.UtcNow.AddDays(-60)
        };

    // -----------------------------------------------------------------------
    // Tests: no data → null estimate
    // -----------------------------------------------------------------------

    [Fact]
    public async Task EstimateAsync_NoProfile_ReturnsNull()
    {
        var (svc, _, bodyRepo, goalRepo, _) = BuildService();

        await goalRepo.AddAsync(new FitnessGoal
        {
            UserId = 1,
            GoalType = GoalType.LoseWeight,
            TargetWeightKg = 80
        });
        await bodyRepo.AddAsync(new BodyMetric { UserId = 1, WeightKg = 90 });

        var result = await svc.EstimateAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task EstimateAsync_NoGoal_ReturnsNull()
    {
        var (svc, userRepo, bodyRepo, _, _) = BuildService();

        await userRepo.AddAsync(MakeProfile());
        await bodyRepo.AddAsync(new BodyMetric { UserId = 1, WeightKg = 90 });

        var result = await svc.EstimateAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task EstimateAsync_NoBodyMetric_ReturnsNull()
    {
        var (svc, userRepo, _, goalRepo, _) = BuildService();

        await userRepo.AddAsync(MakeProfile());
        await goalRepo.AddAsync(new FitnessGoal
        {
            UserId = 1,
            GoalType = GoalType.LoseWeight,
            TargetWeightKg = 80
        });

        var result = await svc.EstimateAsync(1);

        Assert.Null(result);
    }

    // -----------------------------------------------------------------------
    // Tests: already at target
    // -----------------------------------------------------------------------

    [Fact]
    public async Task EstimateAsync_AlreadyAtTarget_ReturnsZeroWeeks()
    {
        var (svc, userRepo, bodyRepo, goalRepo, _) = BuildService();

        await userRepo.AddAsync(MakeProfile(80));
        await goalRepo.AddAsync(new FitnessGoal
        {
            UserId = 1,
            GoalType = GoalType.LoseWeight,
            TargetWeightKg = 80
        });
        await bodyRepo.AddAsync(new BodyMetric { UserId = 1, WeightKg = 80, MeasuredAt = DateTime.UtcNow });

        var result = await svc.EstimateAsync(1);

        Assert.NotNull(result);
        Assert.Equal(0, result.EstimatedWeeksRemaining);
        Assert.Equal(0, result.RemainingKg);
    }

    // -----------------------------------------------------------------------
    // Tests: calorie-based estimate (no trend data)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task EstimateAsync_CalorieBased_LoseWeight_ReturnsPositiveWeeks()
    {
        var (svc, userRepo, bodyRepo, goalRepo, _) = BuildService();

        await userRepo.AddAsync(MakeProfile(90, 175)); // ~2300 kcal TDEE
        await goalRepo.AddAsync(new FitnessGoal
        {
            UserId = 1,
            GoalType = GoalType.LoseWeight,
            TargetWeightKg = 80,          // 10 kg to lose
            DailyCalorieTarget = 1800      // ~500 kcal deficit → ~0.45 kg/week
        });
        await bodyRepo.AddAsync(new BodyMetric
        {
            UserId = 1,
            WeightKg = 90,
            MeasuredAt = DateTime.UtcNow
        });

        var result = await svc.EstimateAsync(1);

        Assert.NotNull(result);
        Assert.True(result.EstimatedWeeksRemaining > 0,
            "Expected positive weeks remaining when weight loss goal is not yet met.");
        Assert.True(result.WeeklyRateKg < 0,
            "Rate should be negative for a weight loss goal.");
    }

    // -----------------------------------------------------------------------
    // Tests: trend-based estimate (8-week progress data)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task EstimateAsync_TrendBased_LoseWeight_UsesProgressData()
    {
        var (svc, userRepo, bodyRepo, goalRepo, _) = BuildService();

        await userRepo.AddAsync(MakeProfile(90));
        await goalRepo.AddAsync(new FitnessGoal
        {
            UserId = 1,
            GoalType = GoalType.LoseWeight,
            TargetWeightKg = 80
        });

        // Add 8 weeks of measurements: losing 0.5 kg/week
        for (int week = 8; week >= 0; week--)
        {
            await bodyRepo.AddAsync(new BodyMetric
            {
                UserId = 1,
                WeightKg = 90 - (8 - week) * 0.5,
                MeasuredAt = DateTime.UtcNow.AddDays(-week * 7)
            });
        }

        var result = await svc.EstimateAsync(1);

        Assert.NotNull(result);
        Assert.InRange(result.WeeklyRateKg, -0.7, -0.3); // ~−0.5 kg/week
        // After 8 weeks losing 0.5 kg/week: current weight = 86, target = 80 → 6 kg remaining
        // 6 kg / 0.5 per week = ~12 weeks
        Assert.InRange(result.EstimatedWeeksRemaining, 9, 16);
    }

    [Fact]
    public async Task EstimateAsync_TrendBased_GainMuscle_PositiveRate()
    {
        var (svc, userRepo, bodyRepo, goalRepo, _) = BuildService();

        await userRepo.AddAsync(MakeProfile(70));
        await goalRepo.AddAsync(new FitnessGoal
        {
            UserId = 1,
            GoalType = GoalType.GainMuscle,
            TargetWeightKg = 80
        });

        // 8 weeks of measurements: gaining 0.3 kg/week
        for (int week = 8; week >= 0; week--)
        {
            await bodyRepo.AddAsync(new BodyMetric
            {
                UserId = 1,
                WeightKg = 70 + (8 - week) * 0.3,
                MeasuredAt = DateTime.UtcNow.AddDays(-week * 7)
            });
        }

        var result = await svc.EstimateAsync(1);

        Assert.NotNull(result);
        Assert.InRange(result.WeeklyRateKg, 0.2, 0.4);
    }

    // -----------------------------------------------------------------------
    // Tests: IsOnTrack
    // -----------------------------------------------------------------------

    [Fact]
    public async Task EstimateAsync_IsOnTrack_TrueWhenCompletionBeforeTargetDate()
    {
        var (svc, userRepo, bodyRepo, goalRepo, _) = BuildService();

        await userRepo.AddAsync(MakeProfile(90));
        await goalRepo.AddAsync(new FitnessGoal
        {
            UserId = 1,
            GoalType = GoalType.LoseWeight,
            TargetWeightKg = 80,
            TargetDate = DateTime.UtcNow.AddDays(200) // generous deadline
        });

        for (int week = 8; week >= 0; week--)
        {
            await bodyRepo.AddAsync(new BodyMetric
            {
                UserId = 1,
                WeightKg = 90 - (8 - week) * 0.5,
                MeasuredAt = DateTime.UtcNow.AddDays(-week * 7)
            });
        }

        var result = await svc.EstimateAsync(1);

        Assert.NotNull(result);
        Assert.True(result.IsOnTrack);
    }

    [Fact]
    public async Task EstimateAsync_IsOnTrack_FalseWhenCompletionAfterTargetDate()
    {
        var (svc, userRepo, bodyRepo, goalRepo, _) = BuildService();

        await userRepo.AddAsync(MakeProfile(90));
        await goalRepo.AddAsync(new FitnessGoal
        {
            UserId = 1,
            GoalType = GoalType.LoseWeight,
            TargetWeightKg = 80,
            TargetDate = DateTime.UtcNow.AddDays(14) // very tight deadline
        });

        // Slow progress: only 0.1 kg/week loss → 100 weeks needed
        for (int week = 8; week >= 0; week--)
        {
            await bodyRepo.AddAsync(new BodyMetric
            {
                UserId = 1,
                WeightKg = 90 - (8 - week) * 0.1,
                MeasuredAt = DateTime.UtcNow.AddDays(-week * 7)
            });
        }

        var result = await svc.EstimateAsync(1);

        Assert.NotNull(result);
        Assert.False(result.IsOnTrack);
    }
}
