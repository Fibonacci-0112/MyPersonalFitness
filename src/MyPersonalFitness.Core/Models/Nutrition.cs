namespace MyPersonalFitness.Core.Models;

/// <summary>
/// A food item in the food database, with macronutrient information per 100 g serving.
/// </summary>
public class FoodItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }

    // Per 100 g
    public double CaloriesPer100g { get; set; }
    public double ProteinPer100g { get; set; }
    public double CarbsPer100g { get; set; }
    public double FatPer100g { get; set; }
    public double FiberPer100g { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// A specific food entry within a meal, linking a FoodItem with a quantity.
/// </summary>
public class MealFood
{
    public int Id { get; set; }
    public int MealId { get; set; }
    public int FoodItemId { get; set; }
    public string FoodName { get; set; } = string.Empty;

    /// <summary>Quantity consumed in grams.</summary>
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

/// <summary>
/// A meal (breakfast, lunch, dinner, snack) logged for a particular day.
/// </summary>
public class Meal
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public MealType MealType { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
    public List<MealFood> Foods { get; set; } = [];
    public string? Notes { get; set; }

    public double TotalCalories => Foods.Sum(f => f.Calories);
    public double TotalProtein => Foods.Sum(f => f.Protein);
    public double TotalCarbs => Foods.Sum(f => f.Carbs);
    public double TotalFat => Foods.Sum(f => f.Fat);
}

public enum MealType
{
    Breakfast,
    MorningSnack,
    Lunch,
    AfternoonSnack,
    Dinner,
    EveningSnack,
    Other
}
