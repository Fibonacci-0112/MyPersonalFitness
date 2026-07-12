namespace MyPersonalFitness.Core.Models;

/// <summary>
/// Represents a planned workout template (exercise, sets, reps, target weight).
/// Users plan future workouts using this template and then log actual sessions against it.
/// </summary>
public class WorkoutPlan
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<WorkoutPlanExercise> Exercises { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// An exercise entry within a workout plan, specifying target sets, reps and weight.
/// </summary>
public class WorkoutPlanExercise
{
    public int Id { get; set; }
    public int WorkoutPlanId { get; set; }
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int OrderInPlan { get; set; }

    /// <summary>Planned number of sets.</summary>
    public int TargetSets { get; set; }

    /// <summary>Planned number of repetitions per set.</summary>
    public int TargetReps { get; set; }

    /// <summary>Planned weight in kilograms per set. Zero for bodyweight exercises.</summary>
    public double TargetWeightKg { get; set; }

    public string? Notes { get; set; }
}
