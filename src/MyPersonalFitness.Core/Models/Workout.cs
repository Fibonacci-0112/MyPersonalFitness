namespace MyPersonalFitness.Core.Models;

/// <summary>
/// A definition of a physical exercise (e.g., "Bench Press", "Squat").
/// </summary>
public class Exercise
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public MuscleGroup PrimaryMuscleGroup { get; set; }
    public ExerciseCategory Category { get; set; }

    /// <summary>Whether the exercise uses external weight (barbell, dumbbell, machine).</summary>
    public bool UsesWeight { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// One set within an exercise entry of a workout.
/// </summary>
public class WorkoutSet
{
    public int Id { get; set; }
    public int WorkoutExerciseId { get; set; }
    public int SetNumber { get; set; }
    public int Reps { get; set; }

    /// <summary>Weight used for the set in kilograms. Zero for bodyweight exercises.</summary>
    public double WeightKg { get; set; }

    /// <summary>Duration in seconds for time-based sets (planks, holds). Null if rep-based.</summary>
    public int? DurationSeconds { get; set; }

    public bool IsCompleted { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// An exercise entry within a workout session (links Exercise to a Workout and holds its sets).
/// </summary>
public class WorkoutExercise
{
    public int Id { get; set; }
    public int WorkoutId { get; set; }
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public int OrderInWorkout { get; set; }
    public List<WorkoutSet> Sets { get; set; } = [];
    public string? Notes { get; set; }
}

/// <summary>
/// A completed or in-progress workout session.
/// </summary>
public class Workout
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public List<WorkoutExercise> Exercises { get; set; } = [];
    public string? Notes { get; set; }

    /// <summary>Total volume lifted (sum of weight × reps across all sets).</summary>
    public double TotalVolumeKg =>
        Exercises.SelectMany(e => e.Sets)
                 .Where(s => s.IsCompleted)
                 .Sum(s => s.WeightKg * s.Reps);

    public TimeSpan? Duration =>
        CompletedAt.HasValue ? CompletedAt.Value - StartedAt : null;
}

public enum MuscleGroup
{
    Chest,
    Back,
    Shoulders,
    Biceps,
    Triceps,
    Forearms,
    Core,
    Glutes,
    Quadriceps,
    Hamstrings,
    Calves,
    FullBody,
    Other
}

public enum ExerciseCategory
{
    Strength,
    Cardio,
    Flexibility,
    Balance,
    Plyometric,
    Other
}
