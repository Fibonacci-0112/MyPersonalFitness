using MyPersonalFitness.Core.Models;

namespace MyPersonalFitness.Core.Interfaces;

public interface IMealRepository : IRepository<Meal>
{
    Task<IEnumerable<Meal>> GetByUserAsync(int userId);
    Task<IEnumerable<Meal>> GetByUserAndDateAsync(int userId, DateTime date);
    Task<IEnumerable<Meal>> GetByUserAndDateRangeAsync(int userId, DateTime from, DateTime to);
}

public interface IFoodItemRepository : IRepository<FoodItem>
{
    Task<IEnumerable<FoodItem>> SearchAsync(string query);
}

public interface IMealPlanRepository : IRepository<MealPlan>
{
    Task<IEnumerable<MealPlan>> GetByUserAsync(int userId);
    Task<IEnumerable<MealPlan>> GetActiveByUserAsync(int userId);
}
