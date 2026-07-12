using MyPersonalFitness.Core.Data;
using MyPersonalFitness.Core.Models;
using MyPersonalFitness.Core.Services;

namespace MyPersonalFitness.Tests.Services;

public class NutritionServiceTests
{
    private static NutritionService BuildService()
    {
        return new NutritionService(
            new InMemoryMealRepository(),
            new InMemoryFoodItemRepository(),
            new InMemoryMealPlanRepository());
    }

    [Fact]
    public async Task LogMealAsync_StoresMeal()
    {
        var svc = BuildService();
        var meal = new Meal
        {
            UserId = 1,
            MealType = MealType.Breakfast,
            LoggedAt = DateTime.UtcNow
        };

        var saved = await svc.LogMealAsync(meal);

        Assert.NotEqual(0, saved.Id);
    }

    [Fact]
    public async Task GetDailySummaryAsync_SumsCaloriesCorrectly()
    {
        var svc = BuildService();
        var food = new FoodItem
        {
            Name = "Oats",
            CaloriesPer100g = 389,
            ProteinPer100g = 17,
            CarbsPer100g = 66,
            FatPer100g = 7,
            FiberPer100g = 10
        };
        await svc.AddFoodItemAsync(food);

        var today = DateTime.UtcNow;
        await svc.LogMealAsync(new Meal
        {
            UserId = 1,
            MealType = MealType.Breakfast,
            LoggedAt = today,
            Foods =
            [
                new MealFood
                {
                    FoodName = food.Name,
                    QuantityGrams = 100,
                    FoodItem = food
                }
            ]
        });

        var summary = await svc.GetDailySummaryAsync(1, today);

        Assert.Equal(1, summary.MealCount);
        Assert.Equal(389, summary.TotalCalories, precision: 0);
    }

    [Fact]
    public async Task GetMealsForDayAsync_FiltersCorrectly()
    {
        var svc = BuildService();
        var today = DateTime.UtcNow;
        var yesterday = today.AddDays(-1);

        await svc.LogMealAsync(new Meal { UserId = 1, MealType = MealType.Lunch, LoggedAt = today });
        await svc.LogMealAsync(new Meal { UserId = 1, MealType = MealType.Dinner, LoggedAt = yesterday });

        var meals = (await svc.GetMealsForDayAsync(1, today)).ToList();

        Assert.Single(meals);
        Assert.Equal(MealType.Lunch, meals[0].MealType);
    }

    [Fact]
    public async Task CreateMealPlanAsync_StoresPlan()
    {
        var svc = BuildService();
        var plan = new MealPlan
        {
            UserId = 1,
            Name = "2000 kcal Bulk"
        };

        var saved = await svc.CreateMealPlanAsync(plan);

        Assert.NotEqual(0, saved.Id);
        var plans = (await svc.GetMealPlansAsync(1)).ToList();
        Assert.Single(plans);
    }

    [Fact]
    public async Task MealFood_CaloriesCalculatedFromFoodItem()
    {
        var food = new FoodItem { CaloriesPer100g = 100, ProteinPer100g = 10, CarbsPer100g = 10, FatPer100g = 5 };
        var entry = new MealFood { QuantityGrams = 200, FoodItem = food };

        Assert.Equal(200, entry.Calories);
        Assert.Equal(20, entry.Protein);
        Assert.Equal(10, entry.Fat);
    }
}
