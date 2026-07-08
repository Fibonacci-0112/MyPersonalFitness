# Copilot Instructions – MyPersonalFitness

## What this repository is

**MyPersonalFitness** is a cross-platform health and fitness app targeting Windows, Android, iOS, macOS and Web. It lets users track workouts, log nutrition/meals, record body metrics, plan training/diet, and estimate progress towards a fitness goal.

---

## Repository layout

```
MyPersonalFitness.slnx          ← solution file (dotnet slnx format)
src/
  MyPersonalFitness.Core/       ← .NET 10 class library – all shared logic
    Models/                     ← Domain models (plain C# classes / records)
    Interfaces/                 ← Repository contracts (all async)
    Services/                   ← Business-logic services (injected via DI)
    Data/                       ← In-memory repository implementations (default/test)
    ServiceCollectionExtensions.cs  ← AddMyPersonalFitnessCore() extension method
  MyPersonalFitness.Web/        ← Blazor WebAssembly (browser, no server)
    Pages/                      ← Razor pages (.razor files)
    Layout/                     ← Shell / nav layout
    Program.cs                  ← WASM startup, calls AddMyPersonalFitnessCore()
  MyPersonalFitness.App/        ← .NET MAUI (Windows / Android / iOS / macOS)
    Pages/                      ← XAML pages
    ViewModels/                 ← MVVM view-models (CommunityToolkit.Mvvm)
    MauiProgram.cs              ← MAUI startup, calls AddMyPersonalFitnessCore()
tests/
  MyPersonalFitness.Tests/      ← xUnit unit tests for Core services only
    Services/                   ← One test class per service
```

---

## Architecture & dependency injection

### Core pattern

`AddMyPersonalFitnessCore()` (in `ServiceCollectionExtensions.cs`) is the single registration entry-point called by **both** Web and App startups. It registers:

- All repository interfaces → `InMemory*` implementations (singletons, default)
- All services → scoped (`WorkoutService`, `NutritionService`, `BodyMetricService`, `GoalEstimatorService`)

Platform projects may **override** individual repository registrations after calling `AddMyPersonalFitnessCore()`. The MAUI project's comment in `MauiProgram.cs` shows the pattern:
```csharp
builder.Services.AddMyPersonalFitnessCore();
// Override with SQLite:
// builder.Services.AddSingleton<IWorkoutRepository, SqliteWorkoutRepository>();
```

### Repository contracts

All repositories extend the generic `IRepository<T>` interface (`GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`). Domain-specific interfaces add extra query methods, e.g.:
- `IWorkoutRepository` – `GetByUserAsync`, `GetByUserAndDateRangeAsync`
- `IBodyMetricRepository` – `GetByUserAsync`, `GetByUserAndDateRangeAsync`, `GetLatestByUserAsync`
- `IMealRepository` – `GetByUserAsync`, `GetByUserAndDateAsync`, `GetByUserAndDateRangeAsync`
- `IUserProfileRepository` – `GetCurrentUserAsync`
- `IFitnessGoalRepository` – `GetByUserAsync`, `GetActiveGoalByUserAsync`

Every repository method is **async** (returns `Task` or `Task<T>`). The `InMemoryRepository<T>` base class wraps a `List<T>` behind `Task.FromResult` – it is not thread-safe for concurrent writers.

### Adding a new domain entity

1. Add a model class in `src/MyPersonalFitness.Core/Models/`.
2. Add an interface in `src/MyPersonalFitness.Core/Interfaces/` extending `IRepository<T>`.
3. Add an `InMemory*Repository` in `src/MyPersonalFitness.Core/Data/` extending `InMemoryRepository<T>`.
4. Register both in `AddMyPersonalFitnessCore()`.
5. Add a service in `src/MyPersonalFitness.Core/Services/` if business logic is needed.
6. Add a Blazor page in `src/MyPersonalFitness.Web/Pages/` and/or a MAUI page + ViewModel pair.
7. Add unit tests in `tests/MyPersonalFitness.Tests/Services/`.

---

## Domain model quick reference

| Model | Key fields | Notes |
|-------|-----------|-------|
| `UserProfile` | `Id`, `Name`, `DateOfBirth`, `Gender`, `HeightCm`, `StartingWeightKg`, `ActivityLevel` | Single "current user" (`GetCurrentUserAsync`) |
| `Workout` | `UserId`, `Name`, `StartedAt`, `CompletedAt`, `Exercises` | `TotalVolumeKg` computed from completed sets |
| `WorkoutExercise` | `WorkoutId`, `ExerciseId`, `ExerciseName`, `Sets` | Child of `Workout` |
| `WorkoutSet` | `Reps`, `WeightKg`, `DurationSeconds`, `IsCompleted` | `WeightKg = 0` for bodyweight exercises |
| `Exercise` | `Name`, `PrimaryMuscleGroup`, `Category`, `UsesWeight` | Catalogue entry |
| `WorkoutPlan` | `UserId`, `Name`, `Exercises`, `IsActive` | Template for future workouts |
| `Meal` | `UserId`, `MealType`, `LoggedAt`, `Foods` | `TotalCalories/Protein/Carbs/Fat` computed |
| `MealFood` | `MealId`, `FoodItemId`, `QuantityGrams`, `FoodItem?` | Macro calcs from `FoodItem` per 100 g |
| `FoodItem` | `Name`, `CaloriesPer100g`, `Protein/Carbs/Fat/FiberPer100g` | Food database entry |
| `MealPlan` | `UserId`, `Name`, `Entries`, `IsActive` | Template for nutrition planning |
| `BodyMetric` | `UserId`, `MeasuredAt`, `WeightKg`, optional body-fat/measurements | `CalculateBmi(heightCm)` helper |
| `FitnessGoal` | `UserId`, `GoalType`, `TargetWeightKg`, `DailyCalorieTarget`, `TargetDate`, `IsActive` | Only one active goal per user |

Enumerations: `MuscleGroup`, `ExerciseCategory`, `MealType`, `GoalType`, `Gender`, `ActivityLevel`, `ConfidenceLevel`.

All timestamps use **UTC** (`DateTime.UtcNow`). Weight is always in **kilograms**. Distances/heights in **centimetres**. Calories are **kcal**.

---

## Goal estimation algorithm

`GoalEstimatorService.EstimateAsync(userId)` returns a `GoalEstimate` (or `null` if data is insufficient):

1. **Trend-based (preferred):** uses the last 8 weeks of `BodyMetric` records to derive actual kg/week rate.
2. **Calorie-based (fallback):** calculates TDEE from the Mifflin–St Jeor BMR equation × activity multiplier, then uses average daily calorie intake vs TDEE.
3. **Sanity caps:** bulk ≤ +0.5 kg/week, cut ≥ −1.0 kg/week; the rate must agree with goal direction.
4. `ConfidenceLevel` is `High` (≤3 days since last measurement), `Medium` (≤14 days), or `Low` (older).

---

## Build, run & test commands

```bash
# Build Core (no workload needed)
dotnet build src/MyPersonalFitness.Core

# Build & run the Web app (Blazor WASM – no MAUI needed)
dotnet build src/MyPersonalFitness.Web
cd src/MyPersonalFitness.Web && dotnet run

# Run unit tests (only tests Core; no MAUI workload needed)
dotnet test tests/MyPersonalFitness.Tests

# Build MAUI (requires: dotnet workload install maui + platform SDK)
dotnet build src/MyPersonalFitness.App -f net10.0-android
dotnet build src/MyPersonalFitness.App -f net10.0-windows10.0.19041.0
dotnet build src/MyPersonalFitness.App -f net10.0-maccatalyst   # macOS only
dotnet build src/MyPersonalFitness.App -f net10.0-ios           # macOS only
```

**Known environment issue:** The MAUI project (`MyPersonalFitness.App`) cannot be built in this sandboxed environment without the MAUI workload. Always build/test using Core and Web targets. Avoid running `dotnet build MyPersonalFitness.slnx` (solution-level) as it will attempt to build the MAUI project and fail without the workload.

---

## Technology stack

| Concern | Technology |
|---------|-----------|
| Target framework | .NET 10 |
| Web UI | Blazor WebAssembly |
| Native UI | .NET MAUI |
| Native state | MVVM + `CommunityToolkit.Mvvm` (`[ObservableProperty]`, `[RelayCommand]`) |
| Web persistence | `Blazored.LocalStorage` (browser localStorage) |
| Native persistence (planned) | `sqlite-net-pcl` via SQLite repository overrides |
| Tests | xUnit (no mocking library – uses `InMemory*` repos directly) |

---

## Coding conventions

- **C# 12 / .NET 10** features are in use: primary constructors on services, collection expressions (`[.. list]`, `[]`), `record` types for DTOs.
- **Nullable reference types** are enabled (`<Nullable>enable</Nullable>`) in all projects – always handle nullable returns.
- **No mocking library** – unit tests instantiate real `InMemory*Repository` instances directly (see `WorkoutServiceTests.BuildService()`).
- Services use **primary constructor injection** (C# 12 syntax).
- Repository `Add` returns the assigned `int` id.
- All public service methods and interface members have XML doc comments (`<summary>`).
- UserId is hard-coded as `1` in the current Web pages and ViewModels (single-user app prototype).
- The `InMemoryRepository<T>` is not thread-safe; do not rely on concurrent writes in tests.
- MAUI ViewModels inherit `ObservableObject`; use `[ObservableProperty]` for bindable fields and `[RelayCommand]` for commands.

---

## Testing approach

- All tests are in `tests/MyPersonalFitness.Tests/Services/`.
- One test file per service class: `WorkoutServiceTests`, `NutritionServiceTests`, `BodyMetricServiceTests`, `GoalEstimatorServiceTests`.
- Tests construct services with `new` (no DI container) by calling a private `BuildService()` factory that wires in `InMemory*Repository` instances.
- Tests are async (`Task`) and use `Assert.*` from xUnit.
- When adding a new service, follow the existing pattern: add a `*ServiceTests.cs` file with a `BuildService()` helper.
