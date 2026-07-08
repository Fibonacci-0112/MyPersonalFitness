using MyPersonalFitness.Core.Interfaces;
using MyPersonalFitness.Core.Models;

namespace MyPersonalFitness.Core.Data;

public class InMemoryMealRepository : InMemoryRepository<Meal>, IMealRepository
{
    public InMemoryMealRepository()
        : base(m => m.Id, (m, id) => m.Id = id) { }

    public async Task<IEnumerable<Meal>> GetByUserAsync(int userId)
    {
        var all = await GetAllAsync();
        return all.Where(m => m.UserId == userId)
                  .OrderByDescending(m => m.LoggedAt);
    }

    public async Task<IEnumerable<Meal>> GetByUserAndDateAsync(int userId, DateTime date)
    {
        var all = await GetAllAsync();
        return all.Where(m => m.UserId == userId
                           && m.LoggedAt.Date == date.Date)
                  .OrderBy(m => m.LoggedAt);
    }

    public async Task<IEnumerable<Meal>> GetByUserAndDateRangeAsync(
        int userId, DateTime from, DateTime to)
    {
        var all = await GetAllAsync();
        return all.Where(m => m.UserId == userId
                           && m.LoggedAt >= from
                           && m.LoggedAt <= to)
                  .OrderByDescending(m => m.LoggedAt);
    }
}

public class InMemoryFoodItemRepository : InMemoryRepository<FoodItem>, IFoodItemRepository
{
    public InMemoryFoodItemRepository()
        : base(f => f.Id, (f, id) => f.Id = id) { }

    public async Task<IEnumerable<FoodItem>> SearchAsync(string query)
    {
        var all = await GetAllAsync();
        return all.Where(f => f.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                           || (f.Brand != null && f.Brand.Contains(query, StringComparison.OrdinalIgnoreCase)));
    }
}

public class InMemoryMealPlanRepository : InMemoryRepository<MealPlan>, IMealPlanRepository
{
    public InMemoryMealPlanRepository()
        : base(p => p.Id, (p, id) => p.Id = id) { }

    public async Task<IEnumerable<MealPlan>> GetByUserAsync(int userId)
    {
        var all = await GetAllAsync();
        return all.Where(p => p.UserId == userId).OrderByDescending(p => p.CreatedAt);
    }

    public async Task<IEnumerable<MealPlan>> GetActiveByUserAsync(int userId)
    {
        var all = await GetByUserAsync(userId);
        return all.Where(p => p.IsActive);
    }
}
