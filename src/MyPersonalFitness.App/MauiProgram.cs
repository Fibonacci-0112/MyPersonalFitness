using Microsoft.Extensions.Logging;
using MyPersonalFitness.Core;

namespace MyPersonalFitness.App;

/// <summary>
/// Entry point for the .NET MAUI application.
/// Registers platform-specific services and Core services via DI.
/// </summary>
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>();

        // Register Core services and default in-memory repositories.
        // In a production build you would override repositories with SQLite implementations
        // (e.g. services.AddSingleton<IWorkoutRepository, SqliteWorkoutRepository>()).
        builder.Services.AddMyPersonalFitnessCore();

        // Register pages for dependency injection
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<WorkoutsPage>();
        builder.Services.AddTransient<NutritionPage>();
        builder.Services.AddTransient<BodyMetricsPage>();
        builder.Services.AddTransient<GoalsPage>();
        builder.Services.AddTransient<ProfilePage>();

        // Register ViewModels
        builder.Services.AddTransient<ViewModels.WorkoutsViewModel>();
        builder.Services.AddTransient<ViewModels.NutritionViewModel>();
        builder.Services.AddTransient<ViewModels.BodyMetricsViewModel>();
        builder.Services.AddTransient<ViewModels.GoalsViewModel>();
        builder.Services.AddTransient<ViewModels.ProfileViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
