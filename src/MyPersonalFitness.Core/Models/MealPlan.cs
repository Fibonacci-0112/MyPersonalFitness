namespace MyPersonalFitness.Core.Models;

/// <summary>
/// Represents a user's planned meal template for nutrition planning.
/// </summary>
public class MealPlan
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<MealPlanEntry> Entries { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public double TotalCalories => Entries.Sum(e => e.Calories);
    public double TotalProtein => Entries.Sum(e => e.Protein);
    public double TotalCarbs => Entries.Sum(e => e.Carbs);
    public double TotalFat => Entries.Sum(e => e.Fat);
}

/// <summary>
/// One food entry within a meal plan (food item + quantity + meal type).
/// </summary>
public class MealPlanEntry
{
    public int Id { get; set; }
    public int MealPlanId { get; set; }
    public int FoodItemId { get; set; }
    public string FoodName { get; set; } = string.Empty;
    public MealType MealType { get; set; }
    public double QuantityGrams { get; set; }

    public double Calories => FoodItem != null
        ? Math.Round(FoodItem.CaloriesPer100g * QuantityGrams / 100, 1)
        : 0;

    public double Protein => FoodItem != null
        ? Math.Round(FoodItem.ProteinPer100g * QuantityGrams / 100, 1)
        : 0;

    public double Carbs => FoodItem != null
        ? Math.Round(FoodItem.CarbsPer100g * QuantityGrams / 100, 1)
        : 0;

    public double Fat => FoodItem != null
        ? Math.Round(FoodItem.FatPer100g * QuantityGrams / 100, 1)
        : 0;

    /// <summary>Navigation property – may be null when not loaded.</summary>
    public FoodItem? FoodItem { get; set; }
}
