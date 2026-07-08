using MyPersonalFitness.Core.Interfaces;
using MyPersonalFitness.Core.Models;

namespace MyPersonalFitness.Core.Services;

/// <summary>
/// Handles logging meals and managing meal plans.
/// </summary>
public class NutritionService(
    IMealRepository mealRepo,
    IFoodItemRepository foodRepo,
    IMealPlanRepository mealPlanRepo)
{
    public async Task<Meal> LogMealAsync(Meal meal)
    {
        meal.LoggedAt = meal.LoggedAt == default ? DateTime.UtcNow : meal.LoggedAt;
        await mealRepo.AddAsync(meal);
        return meal;
    }

    public Task<IEnumerable<Meal>> GetMealsForDayAsync(int userId, DateTime date) =>
        mealRepo.GetByUserAndDateAsync(userId, date);

    public Task<IEnumerable<Meal>> GetMealsForRangeAsync(int userId, DateTime from, DateTime to) =>
        mealRepo.GetByUserAndDateRangeAsync(userId, from, to);

    public async Task<DailySummary> GetDailySummaryAsync(int userId, DateTime date)
    {
        var meals = (await mealRepo.GetByUserAndDateAsync(userId, date)).ToList();
        return new DailySummary
        {
            Date = date.Date,
            TotalCalories = meals.Sum(m => m.TotalCalories),
            TotalProtein = meals.Sum(m => m.TotalProtein),
            TotalCarbs = meals.Sum(m => m.TotalCarbs),
            TotalFat = meals.Sum(m => m.TotalFat),
            MealCount = meals.Count
        };
    }

    public Task<IEnumerable<FoodItem>> SearchFoodsAsync(string query) =>
        foodRepo.SearchAsync(query);

    public async Task<FoodItem> AddFoodItemAsync(FoodItem item)
    {
        await foodRepo.AddAsync(item);
        return item;
    }

    public Task<IEnumerable<MealPlan>> GetMealPlansAsync(int userId) =>
        mealPlanRepo.GetByUserAsync(userId);

    public async Task<MealPlan> CreateMealPlanAsync(MealPlan plan)
    {
        plan.CreatedAt = DateTime.UtcNow;
        plan.UpdatedAt = DateTime.UtcNow;
        await mealPlanRepo.AddAsync(plan);
        return plan;
    }

    public async Task UpdateMealPlanAsync(MealPlan plan)
    {
        plan.UpdatedAt = DateTime.UtcNow;
        await mealPlanRepo.UpdateAsync(plan);
    }

    /// <summary>
    /// Returns average daily calories over the specified date range.
    /// </summary>
    public async Task<double> GetAverageDailyCaloriesAsync(int userId, DateTime from, DateTime to)
    {
        var meals = (await mealRepo.GetByUserAndDateRangeAsync(userId, from, to)).ToList();
        if (!meals.Any()) return 0;

        var days = (to.Date - from.Date).Days + 1;
        return Math.Round(meals.Sum(m => m.TotalCalories) / days, 0);
    }
}

/// <summary>
/// A daily nutritional summary.
/// </summary>
public record DailySummary
{
    public DateTime Date { get; init; }
    public double TotalCalories { get; init; }
    public double TotalProtein { get; init; }
    public double TotalCarbs { get; init; }
    public double TotalFat { get; init; }
    public int MealCount { get; init; }
}
