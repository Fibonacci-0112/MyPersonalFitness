namespace MyPersonalFitness.Core.Models;

/// <summary>
/// Represents a user's fitness goal (e.g., lose weight, gain muscle, maintain).
/// </summary>
public class FitnessGoal
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public GoalType GoalType { get; set; }

    /// <summary>Target weight in kilograms (for weight-based goals).</summary>
    public double? TargetWeightKg { get; set; }

    /// <summary>Target body fat percentage (optional).</summary>
    public double? TargetBodyFatPercent { get; set; }

    /// <summary>Daily calorie target derived from goal and TDEE.</summary>
    public double? DailyCalorieTarget { get; set; }

    /// <summary>Weekly workout frequency target.</summary>
    public int? WeeklyWorkoutsTarget { get; set; }

    /// <summary>Target date desired by the user (optional, for deadline-based goals).</summary>
    public DateTime? TargetDate { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum GoalType
{
    LoseWeight,
    GainMuscle,
    Maintain,
    ImproveEndurance,
    ImproveStrength,
    ImproveFlexibility,
    General
}
