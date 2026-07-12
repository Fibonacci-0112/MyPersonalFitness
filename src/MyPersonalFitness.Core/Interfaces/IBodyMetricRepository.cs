using MyPersonalFitness.Core.Models;

namespace MyPersonalFitness.Core.Interfaces;

public interface IBodyMetricRepository : IRepository<BodyMetric>
{
    Task<IEnumerable<BodyMetric>> GetByUserAsync(int userId);
    Task<IEnumerable<BodyMetric>> GetByUserAndDateRangeAsync(int userId, DateTime from, DateTime to);
    Task<BodyMetric?> GetLatestByUserAsync(int userId);
}

public interface IUserProfileRepository : IRepository<UserProfile>
{
    Task<UserProfile?> GetCurrentUserAsync();
}

public interface IFitnessGoalRepository : IRepository<FitnessGoal>
{
    Task<IEnumerable<FitnessGoal>> GetByUserAsync(int userId);
    Task<FitnessGoal?> GetActiveGoalByUserAsync(int userId);
}
