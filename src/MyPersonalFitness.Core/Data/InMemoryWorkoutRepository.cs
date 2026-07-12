using MyPersonalFitness.Core.Interfaces;
using MyPersonalFitness.Core.Models;

namespace MyPersonalFitness.Core.Data;

public class InMemoryWorkoutRepository : InMemoryRepository<Workout>, IWorkoutRepository
{
    public InMemoryWorkoutRepository()
        : base(w => w.Id, (w, id) => w.Id = id) { }

    public async Task<IEnumerable<Workout>> GetByUserAsync(int userId)
    {
        var all = await GetAllAsync();
        return all.Where(w => w.UserId == userId)
                  .OrderByDescending(w => w.StartedAt);
    }

    public async Task<IEnumerable<Workout>> GetByUserAndDateRangeAsync(
        int userId, DateTime from, DateTime to)
    {
        var all = await GetAllAsync();
        return all.Where(w => w.UserId == userId
                           && w.StartedAt >= from
                           && w.StartedAt <= to)
                  .OrderByDescending(w => w.StartedAt);
    }
}

public class InMemoryWorkoutPlanRepository : InMemoryRepository<WorkoutPlan>, IWorkoutPlanRepository
{
    public InMemoryWorkoutPlanRepository()
        : base(p => p.Id, (p, id) => p.Id = id) { }

    public async Task<IEnumerable<WorkoutPlan>> GetByUserAsync(int userId)
    {
        var all = await GetAllAsync();
        return all.Where(p => p.UserId == userId).OrderByDescending(p => p.CreatedAt);
    }

    public async Task<IEnumerable<WorkoutPlan>> GetActiveByUserAsync(int userId)
    {
        var all = await GetByUserAsync(userId);
        return all.Where(p => p.IsActive);
    }
}

public class InMemoryExerciseRepository : InMemoryRepository<Exercise>, IExerciseRepository
{
    public InMemoryExerciseRepository()
        : base(e => e.Id, (e, id) => e.Id = id) { }

    public async Task<IEnumerable<Exercise>> SearchAsync(string query)
    {
        var all = await GetAllAsync();
        return all.Where(e => e.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IEnumerable<Exercise>> GetByMuscleGroupAsync(MuscleGroup muscleGroup)
    {
        var all = await GetAllAsync();
        return all.Where(e => e.PrimaryMuscleGroup == muscleGroup);
    }
}
