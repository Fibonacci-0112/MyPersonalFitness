# MyPersonalFitness

A cross-platform health and fitness app that runs on **Windows, Android, iOS, macOS and Web**. Built to help you track workouts, nutrition/meals and body metrics, plan your training and diet, and estimate how long it will take to reach your fitness goal.

## Features

| Feature | Description |
|---------|-------------|
| 🏋️ **Workout Tracking** | Log completed workouts with exercises, sets, reps and weight lifted |
| 📋 **Workout Planning** | Create reusable workout plan templates (exercise, sets, reps, target weight) |
| 🥗 **Nutrition Tracking** | Log meals by type (breakfast, lunch, dinner, snacks) with per-food calorie/macro data |
| 🍽️ **Meal Planning** | Build meal plan templates to hit daily calorie and macro targets |
| 📊 **Body Metrics** | Record weight, body fat %, waist/hip/chest measurements over time |
| 🎯 **Goal Setting** | Set weight loss, muscle gain or maintenance goals with optional target dates |
| ⏱️ **Goal Estimation** | Estimates time to goal based on your starting point, real progress data and calorie intake |

## Architecture

```
MyPersonalFitness.sln
├── src/
│   ├── MyPersonalFitness.Core/        # Shared models, services, repository interfaces
│   │   ├── Models/                    # Domain models (Workout, Meal, BodyMetric, Goal …)
│   │   ├── Interfaces/                # Repository contracts (IWorkoutRepository …)
│   │   ├── Services/                  # Business logic (WorkoutService, GoalEstimatorService …)
│   │   └── Data/                      # In-memory repository implementations
│   ├── MyPersonalFitness.Web/         # Blazor WebAssembly – runs in any browser
│   └── MyPersonalFitness.App/         # .NET MAUI – Windows, Android, iOS, macOS
└── tests/
    └── MyPersonalFitness.Tests/       # xUnit unit tests for Core services
```

### Technology choices

| Layer | Technology | Reason |
|-------|-----------|--------|
| Shared logic | .NET 11 class library | Single codebase shared across all platforms |
| Web | Blazor WebAssembly | Runs in browser with no server; PWA-ready |
| Native (Win/Android/iOS/macOS) | .NET MAUI | Microsoft's official cross-platform UI framework |
| Native state | MVVM + CommunityToolkit.Mvvm | Reactive UI with minimal boilerplate |
| Persistence (Web) | Blazored.LocalStorage | Browser localStorage, no server needed |
| Persistence (Native) | SQLite via sqlite-net-pcl* | Lightweight embedded database |
| Tests | xUnit | Standard .NET testing framework |

\* The MAUI project defines repository interfaces in Core; SQLite implementations are wired in `MauiProgram.cs` via dependency injection.

## Getting started

### Prerequisites

* [.NET 11 SDK](https://dotnet.microsoft.com/download) matching the version pinned in `global.json` (preview SDKs may be required)
* .NET MAUI workload (for native platforms): `dotnet workload install maui`

### Run the Web app (no MAUI workload required)

```bash
cd src/MyPersonalFitness.Web
dotnet run
# Open https://localhost:5xxx in your browser
```

### Run tests

```bash
dotnet test tests/MyPersonalFitness.Tests
```

### Build the MAUI app (requires MAUI workload + platform SDK)

```bash
# Android
dotnet build src/MyPersonalFitness.App -f net11.0-android

# Windows
dotnet build src/MyPersonalFitness.App -f net11.0-windows10.0.19041.0

# macOS / iOS (requires macOS)
dotnet build src/MyPersonalFitness.App -f net11.0-maccatalyst
dotnet build src/MyPersonalFitness.App -f net11.0-ios
```

## Goal Estimation Algorithm

The `GoalEstimatorService` estimates your time to goal in two stages:

1. **Trend-based (preferred):** Uses up to 8 weeks of real body-weight measurements to calculate your actual rate of change (kg/week).
2. **Calorie-based (fallback):** Uses your logged average daily calorie intake vs. your TDEE (calculated via the Mifflin–St Jeor equation) to project a rate of change.

The resulting timeline, confidence level and weekly calorie adjustment are shown on the Goals & Progress screen.
