using MyPersonalFitness.Core.Interfaces;
using MyPersonalFitness.Core.Models;

namespace MyPersonalFitness.Core.Data;

public class InMemoryBodyMetricRepository : InMemoryRepository<BodyMetric>, IBodyMetricRepository
{
    public InMemoryBodyMetricRepository()
        : base(b => b.Id, (b, id) => b.Id = id) { }

    public async Task<IEnumerable<BodyMetric>> GetByUserAsync(int userId)
    {
        var all = await GetAllAsync();
        return all.Where(b => b.UserId == userId)
                  .OrderByDescending(b => b.MeasuredAt);
    }

    public async Task<IEnumerable<BodyMetric>> GetByUserAndDateRangeAsync(
        int userId, DateTime from, DateTime to)
    {
        var all = await GetAllAsync();
        return all.Where(b => b.UserId == userId
                           && b.MeasuredAt >= from
                           && b.MeasuredAt <= to)
                  .OrderByDescending(b => b.MeasuredAt);
    }

    public async Task<BodyMetric?> GetLatestByUserAsync(int userId)
    {
        var all = await GetByUserAsync(userId);
        return all.FirstOrDefault();
    }
}

public class InMemoryUserProfileRepository : InMemoryRepository<UserProfile>, IUserProfileRepository
{
    public InMemoryUserProfileRepository()
        : base(u => u.Id, (u, id) => u.Id = id) { }

    public async Task<UserProfile?> GetCurrentUserAsync()
    {
        var all = await GetAllAsync();
        return all.FirstOrDefault();
    }
}

public class InMemoryFitnessGoalRepository : InMemoryRepository<FitnessGoal>, IFitnessGoalRepository
{
    public InMemoryFitnessGoalRepository()
        : base(g => g.Id, (g, id) => g.Id = id) { }

    public async Task<IEnumerable<FitnessGoal>> GetByUserAsync(int userId)
    {
        var all = await GetAllAsync();
        return all.Where(g => g.UserId == userId).OrderByDescending(g => g.CreatedAt);
    }

    public async Task<FitnessGoal?> GetActiveGoalByUserAsync(int userId)
    {
        var all = await GetByUserAsync(userId);
        return all.FirstOrDefault(g => g.IsActive);
    }
}
