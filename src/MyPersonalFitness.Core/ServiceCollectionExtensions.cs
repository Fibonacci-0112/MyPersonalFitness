using Microsoft.Extensions.DependencyInjection;
using MyPersonalFitness.Core.Data;
using MyPersonalFitness.Core.Interfaces;
using MyPersonalFitness.Core.Services;

namespace MyPersonalFitness.Core;

/// <summary>
/// Extension methods for registering Core services and in-memory repositories.
/// Call <see cref="AddMyPersonalFitnessCore"/> in both the Web and MAUI startup.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Core services. Repositories are registered as in-memory singletons
    /// by default; platform projects may override individual repository registrations with
    /// their own persistent implementations after calling this method.
    /// </summary>
    public static IServiceCollection AddMyPersonalFitnessCore(this IServiceCollection services)
    {
        // Repositories – in-memory defaults (override per platform)
        services.AddSingleton<IUserProfileRepository, InMemoryUserProfileRepository>();
        services.AddSingleton<IBodyMetricRepository, InMemoryBodyMetricRepository>();
        services.AddSingleton<IWorkoutRepository, InMemoryWorkoutRepository>();
        services.AddSingleton<IWorkoutPlanRepository, InMemoryWorkoutPlanRepository>();
        services.AddSingleton<IExerciseRepository, InMemoryExerciseRepository>();
        services.AddSingleton<IMealRepository, InMemoryMealRepository>();
        services.AddSingleton<IFoodItemRepository, InMemoryFoodItemRepository>();
        services.AddSingleton<IMealPlanRepository, InMemoryMealPlanRepository>();
        services.AddSingleton<IFitnessGoalRepository, InMemoryFitnessGoalRepository>();

        // Application services
        services.AddScoped<WorkoutService>();
        services.AddScoped<NutritionService>();
        services.AddScoped<BodyMetricService>();
        services.AddScoped<GoalEstimatorService>();

        return services;
    }
}
