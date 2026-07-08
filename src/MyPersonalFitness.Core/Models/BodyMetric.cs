namespace MyPersonalFitness.Core.Models;

/// <summary>
/// A body metric measurement captured at a point in time (weight, body fat %, BMI, etc.).
/// </summary>
public class BodyMetric
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime MeasuredAt { get; set; } = DateTime.UtcNow;

    /// <summary>Weight in kilograms.</summary>
    public double WeightKg { get; set; }

    /// <summary>Body fat percentage (optional).</summary>
    public double? BodyFatPercent { get; set; }

    /// <summary>Muscle mass in kilograms (optional).</summary>
    public double? MuscleMassKg { get; set; }

    /// <summary>Waist circumference in centimetres (optional).</summary>
    public double? WaistCm { get; set; }

    /// <summary>Hip circumference in centimetres (optional).</summary>
    public double? HipCm { get; set; }

    /// <summary>Chest circumference in centimetres (optional).</summary>
    public double? ChestCm { get; set; }

    public string? Notes { get; set; }

    /// <summary>BMI calculated from weight and user height (read-only helper).</summary>
    public double CalculateBmi(double heightCm)
    {
        if (heightCm <= 0) return 0;
        double heightM = heightCm / 100.0;
        return Math.Round(WeightKg / (heightM * heightM), 1);
    }
}
