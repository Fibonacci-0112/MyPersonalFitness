using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyPersonalFitness.Core.Models;
using MyPersonalFitness.Core.Services;

namespace MyPersonalFitness.App.ViewModels;

public partial class WorkoutsViewModel : ObservableObject
{
    private readonly WorkoutService _workoutSvc;

    [ObservableProperty]
    private IEnumerable<Workout> _workouts = [];

    [ObservableProperty]
    private string _newWorkoutName = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    private const int UserId = 1;

    public WorkoutsViewModel(WorkoutService workoutSvc)
    {
        _workoutSvc = workoutSvc;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        Workouts = await _workoutSvc.GetWorkoutHistoryAsync(UserId);
        IsBusy = false;
    }

    [RelayCommand]
    public async Task StartWorkoutAsync()
    {
        if (string.IsNullOrWhiteSpace(NewWorkoutName)) return;

        IsBusy = true;
        var workout = await _workoutSvc.StartWorkoutAsync(UserId, NewWorkoutName);
        await _workoutSvc.CompleteWorkoutAsync(workout.Id);
        NewWorkoutName = string.Empty;
        StatusMessage = $"Workout '{workout.Name}' saved!";
        await LoadAsync();
        IsBusy = false;
    }
}
