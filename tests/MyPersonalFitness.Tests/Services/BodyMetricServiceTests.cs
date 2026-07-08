using MyPersonalFitness.Core.Data;
using MyPersonalFitness.Core.Models;
using MyPersonalFitness.Core.Services;

namespace MyPersonalFitness.Tests.Services;

public class BodyMetricServiceTests
{
    private static (BodyMetricService svc,
                    InMemoryBodyMetricRepository bodyRepo,
                    InMemoryUserProfileRepository userRepo) BuildService()
    {
        var bodyRepo = new InMemoryBodyMetricRepository();
        var userRepo = new InMemoryUserProfileRepository();
        return (new BodyMetricService(bodyRepo, userRepo), bodyRepo, userRepo);
    }

    [Fact]
    public async Task LogBodyMetricAsync_StoresMetric()
    {
        var (svc, _, _) = BuildService();

        var metric = await svc.LogBodyMetricAsync(new BodyMetric { UserId = 1, WeightKg = 85 });

        Assert.NotEqual(0, metric.Id);
    }

    [Fact]
    public async Task GetLatestAsync_ReturnsMostRecentMeasurement()
    {
        var (svc, _, _) = BuildService();

        await svc.LogBodyMetricAsync(new BodyMetric
            { UserId = 1, WeightKg = 90, MeasuredAt = DateTime.UtcNow.AddDays(-10) });
        await svc.LogBodyMetricAsync(new BodyMetric
            { UserId = 1, WeightKg = 88, MeasuredAt = DateTime.UtcNow.AddDays(-3) });
        await svc.LogBodyMetricAsync(new BodyMetric
            { UserId = 1, WeightKg = 87, MeasuredAt = DateTime.UtcNow });

        var latest = await svc.GetLatestAsync(1);

        Assert.NotNull(latest);
        Assert.Equal(87, latest.WeightKg);
    }

    [Fact]
    public async Task GetTotalWeightChangeAsync_ReturnsCorrectDelta()
    {
        var (svc, _, userRepo) = BuildService();

        await userRepo.AddAsync(new UserProfile
        {
            Name = "Test",
            StartingWeightKg = 90,
            HeightCm = 175,
            DateOfBirth = DateTime.UtcNow.AddYears(-30),
            Gender = Gender.Male
        });
        await svc.LogBodyMetricAsync(new BodyMetric
            { UserId = 1, WeightKg = 85, MeasuredAt = DateTime.UtcNow });

        var change = await svc.GetTotalWeightChangeAsync(1);

        Assert.Equal(-5, change);
    }

    [Fact]
    public async Task CalculateBmi_ReturnsReasonableValue()
    {
        var metric = new BodyMetric { WeightKg = 80 };
        var bmi = metric.CalculateBmi(180); // 80 / (1.8^2) ≈ 24.7

        Assert.InRange(bmi, 24.0, 25.5);
    }

    [Fact]
    public async Task CalculateTdeeAsync_ReturnsReasonableEstimate()
    {
        var (svc, _, userRepo) = BuildService();

        await userRepo.AddAsync(new UserProfile
        {
            Name = "Test",
            StartingWeightKg = 80,
            HeightCm = 175,
            DateOfBirth = DateTime.UtcNow.AddYears(-30),
            Gender = Gender.Male,
            ActivityLevel = ActivityLevel.ModeratelyActive
        });

        var tdee = await svc.CalculateTdeeAsync(1);

        // A 30-year-old 80 kg, 175 cm male, moderately active = ~2600-2800 kcal TDEE
        Assert.NotNull(tdee);
        Assert.InRange(tdee.Value, 2200, 3200);
    }
}
