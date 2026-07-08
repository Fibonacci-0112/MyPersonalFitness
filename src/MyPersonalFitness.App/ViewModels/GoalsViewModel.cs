using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyPersonalFitness.Core.Models;
using MyPersonalFitness.Core.Services;

namespace MyPersonalFitness.App.ViewModels;

public partial class GoalsViewModel : ObservableObject
{
    private readonly GoalEstimatorService _estimatorSvc;

    [ObservableProperty]
    private GoalEstimate? _estimate;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _estimateSummary;

    private const int UserId = 1;

    public GoalsViewModel(GoalEstimatorService estimatorSvc)
    {
        _estimatorSvc = estimatorSvc;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        Estimate = await _estimatorSvc.EstimateAsync(UserId);

        EstimateSummary = Estimate == null
            ? "Set up your profile, log a goal and body measurements to see your goal estimate."
            : Estimate.EstimatedWeeksRemaining == 0
                ? "🎉 You've reached your goal!"
                : $"At your current rate ({Estimate.WeeklyRateKg:+0.00;-0.00} kg/wk) you'll reach " +
                  $"{Estimate.TargetWeightKg:F1} kg by {Estimate.EstimatedCompletionDate:MMM d, yyyy} " +
                  $"(~{Estimate.EstimatedWeeksRemaining:F0} weeks). " +
                  (Estimate.IsOnTrack ? "✅ On track!" : "⚠️ Behind schedule.");

        IsBusy = false;
    }
}
