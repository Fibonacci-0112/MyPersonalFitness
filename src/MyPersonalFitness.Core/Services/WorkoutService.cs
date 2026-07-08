using MyPersonalFitness.Core.Interfaces;
using MyPersonalFitness.Core.Models;

namespace MyPersonalFitness.Core.Services;

/// <summary>
/// Handles business logic for logging and planning workouts.
/// </summary>
public class WorkoutService(
    IWorkoutRepository workoutRepo,
    IWorkoutPlanRepository planRepo,
    IExerciseRepository exerciseRepo)
{
    public async Task<Workout> StartWorkoutAsync(int userId, string name, int? planId = null)
    {
        var workout = new Workout
        {
            UserId = userId,
            Name = name,
            StartedAt = DateTime.UtcNow
        };

        if (planId.HasValue)
        {
            var plan = await planRepo.GetByIdAsync(planId.Value);
            if (plan != null)
            {
                workout.Name = string.IsNullOrWhiteSpace(name) ? plan.Name : name;
                workout.Exercises = plan.Exercises
                    .OrderBy(pe => pe.OrderInPlan)
                    .Select((pe, index) => new WorkoutExercise
                    {
                        ExerciseId = pe.ExerciseId,
                        ExerciseName = pe.ExerciseName,
                        OrderInWorkout = index + 1,
                        Sets = Enumerable.Range(1, pe.TargetSets)
                            .Select(s => new WorkoutSet
                            {
                                SetNumber = s,
                                Reps = pe.TargetReps,
                                WeightKg = pe.TargetWeightKg
                            }).ToList()
                    }).ToList();
            }
        }

        await workoutRepo.AddAsync(workout);
        return workout;
    }

    public async Task LogSetAsync(int workoutId, int exerciseIndex, WorkoutSet set)
    {
        var workout = await workoutRepo.GetByIdAsync(workoutId)
            ?? throw new InvalidOperationException($"Workout {workoutId} not found.");

        var exercise = workout.Exercises.ElementAtOrDefault(exerciseIndex)
            ?? throw new InvalidOperationException($"Exercise at index {exerciseIndex} not found.");

        set.IsCompleted = true;
        exercise.Sets.Add(set);
        await workoutRepo.UpdateAsync(workout);
    }

    public async Task CompleteWorkoutAsync(int workoutId)
    {
        var workout = await workoutRepo.GetByIdAsync(workoutId)
            ?? throw new InvalidOperationException($"Workout {workoutId} not found.");

        workout.CompletedAt = DateTime.UtcNow;
        await workoutRepo.UpdateAsync(workout);
    }

    public Task<IEnumerable<Workout>> GetWorkoutHistoryAsync(int userId) =>
        workoutRepo.GetByUserAsync(userId);

    public Task<IEnumerable<Workout>> GetWorkoutHistoryAsync(int userId, DateTime from, DateTime to) =>
        workoutRepo.GetByUserAndDateRangeAsync(userId, from, to);

    public Task<IEnumerable<WorkoutPlan>> GetWorkoutPlansAsync(int userId) =>
        planRepo.GetByUserAsync(userId);

    public async Task<WorkoutPlan> CreateWorkoutPlanAsync(WorkoutPlan plan)
    {
        plan.CreatedAt = DateTime.UtcNow;
        plan.UpdatedAt = DateTime.UtcNow;
        await planRepo.AddAsync(plan);
        return plan;
    }

    public async Task UpdateWorkoutPlanAsync(WorkoutPlan plan)
    {
        plan.UpdatedAt = DateTime.UtcNow;
        await planRepo.UpdateAsync(plan);
    }

    public Task<IEnumerable<Exercise>> SearchExercisesAsync(string query) =>
        exerciseRepo.SearchAsync(query);

    public Task<IEnumerable<Exercise>> GetExercisesByMuscleGroupAsync(MuscleGroup group) =>
        exerciseRepo.GetByMuscleGroupAsync(group);

    /// <summary>
    /// Returns total weekly training volume (sum weight × reps) for the current week.
    /// </summary>
    public async Task<double> GetWeeklyVolumeAsync(int userId)
    {
        var startOfWeek = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
        var workouts = await workoutRepo.GetByUserAndDateRangeAsync(
            userId, startOfWeek, DateTime.UtcNow);
        return workouts.Sum(w => w.TotalVolumeKg);
    }
}
