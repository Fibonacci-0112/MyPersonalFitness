using MyPersonalFitness.Core.Data;
using MyPersonalFitness.Core.Models;
using MyPersonalFitness.Core.Services;

namespace MyPersonalFitness.Tests.Services;

public class WorkoutServiceTests
{
    private static WorkoutService BuildService()
    {
        return new WorkoutService(
            new InMemoryWorkoutRepository(),
            new InMemoryWorkoutPlanRepository(),
            new InMemoryExerciseRepository());
    }

    [Fact]
    public async Task StartWorkoutAsync_CreatesWorkoutWithName()
    {
        var svc = BuildService();

        var workout = await svc.StartWorkoutAsync(1, "Leg Day");

        Assert.NotNull(workout);
        Assert.Equal("Leg Day", workout.Name);
        Assert.Equal(1, workout.UserId);
        Assert.Null(workout.CompletedAt);
    }

    [Fact]
    public async Task CompleteWorkoutAsync_SetsCompletedAt()
    {
        var svc = BuildService();
        var workout = await svc.StartWorkoutAsync(1, "Test");

        await svc.CompleteWorkoutAsync(workout.Id);

        var history = (await svc.GetWorkoutHistoryAsync(1)).ToList();
        Assert.Single(history);
        Assert.NotNull(history[0].CompletedAt);
    }

    [Fact]
    public async Task GetWorkoutHistoryAsync_ReturnsOnlyUserWorkouts()
    {
        var svc = BuildService();
        await svc.StartWorkoutAsync(1, "User 1 Workout A");
        await svc.StartWorkoutAsync(1, "User 1 Workout B");
        await svc.StartWorkoutAsync(2, "User 2 Workout");

        var user1History = (await svc.GetWorkoutHistoryAsync(1)).ToList();

        Assert.Equal(2, user1History.Count);
        Assert.All(user1History, w => Assert.Equal(1, w.UserId));
    }

    [Fact]
    public async Task CreateWorkoutPlanAsync_StoresPlan()
    {
        var svc = BuildService();
        var plan = new WorkoutPlan
        {
            UserId = 1,
            Name = "PPL Push Day",
            Exercises =
            [
                new WorkoutPlanExercise
                {
                    ExerciseName = "Bench Press",
                    TargetSets = 4,
                    TargetReps = 8,
                    TargetWeightKg = 80
                }
            ]
        };

        var saved = await svc.CreateWorkoutPlanAsync(plan);

        Assert.NotEqual(0, saved.Id);
        var plans = (await svc.GetWorkoutPlansAsync(1)).ToList();
        Assert.Single(plans);
        Assert.Equal("PPL Push Day", plans[0].Name);
    }

    [Fact]
    public async Task StartWorkoutAsync_WithPlan_PreloadsExercisesAndSets()
    {
        var svc = BuildService();
        var plan = await svc.CreateWorkoutPlanAsync(new WorkoutPlan
        {
            UserId = 1,
            Name = "Push Day",
            Exercises =
            [
                new WorkoutPlanExercise
                {
                    ExerciseName = "Overhead Press",
                    TargetSets = 3,
                    TargetReps = 10,
                    TargetWeightKg = 50
                }
            ]
        });

        var workout = await svc.StartWorkoutAsync(1, "", plan.Id);

        Assert.Single(workout.Exercises);
        Assert.Equal("Overhead Press", workout.Exercises[0].ExerciseName);
        Assert.Equal(3, workout.Exercises[0].Sets.Count);
        Assert.All(workout.Exercises[0].Sets, s => Assert.Equal(10, s.Reps));
    }

    [Fact]
    public async Task TotalVolumeKg_CalculatesCorrectly()
    {
        var workout = new Workout
        {
            UserId = 1,
            Name = "Test",
            Exercises =
            [
                new WorkoutExercise
                {
                    ExerciseName = "Squat",
                    Sets =
                    [
                        new WorkoutSet { WeightKg = 100, Reps = 5, IsCompleted = true },
                        new WorkoutSet { WeightKg = 100, Reps = 5, IsCompleted = true },
                        new WorkoutSet { WeightKg = 80, Reps = 8, IsCompleted = true }
                    ]
                }
            ]
        };

        // 100×5 + 100×5 + 80×8 = 500 + 500 + 640 = 1640
        Assert.Equal(1640, workout.TotalVolumeKg);
    }
}
