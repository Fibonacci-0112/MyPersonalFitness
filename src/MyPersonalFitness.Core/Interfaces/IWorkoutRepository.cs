using MyPersonalFitness.Core.Models;

namespace MyPersonalFitness.Core.Interfaces;

public interface IWorkoutRepository : IRepository<Workout>
{
    Task<IEnumerable<Workout>> GetByUserAsync(int userId);
    Task<IEnumerable<Workout>> GetByUserAndDateRangeAsync(int userId, DateTime from, DateTime to);
}

public interface IWorkoutPlanRepository : IRepository<WorkoutPlan>
{
    Task<IEnumerable<WorkoutPlan>> GetByUserAsync(int userId);
    Task<IEnumerable<WorkoutPlan>> GetActiveByUserAsync(int userId);
}

public interface IExerciseRepository : IRepository<Exercise>
{
    Task<IEnumerable<Exercise>> SearchAsync(string query);
    Task<IEnumerable<Exercise>> GetByMuscleGroupAsync(MuscleGroup muscleGroup);
}
