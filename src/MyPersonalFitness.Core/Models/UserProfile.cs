namespace MyPersonalFitness.Core.Models;

/// <summary>
/// Represents a user's profile, including starting measurements and personal details.
/// </summary>
public class UserProfile
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public double HeightCm { get; set; }

    // Starting point data (recorded at onboarding)
    public double StartingWeightKg { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;

    // Activity level used in calorie/TDEE calculations
    public ActivityLevel ActivityLevel { get; set; } = ActivityLevel.Sedentary;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum Gender
{
    PreferNotToSay,
    Male,
    Female,
    Other
}

public enum ActivityLevel
{
    Sedentary,          // Little or no exercise
    LightlyActive,      // Light exercise 1-3 days/week
    ModeratelyActive,   // Moderate exercise 3-5 days/week
    VeryActive,         // Hard exercise 6-7 days/week
    ExtraActive         // Very hard exercise / physical job
}
