using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyPersonalFitness.Core.Models;
using MyPersonalFitness.Core.Services;

namespace MyPersonalFitness.App.ViewModels;

public partial class BodyMetricsViewModel : ObservableObject
{
    private readonly BodyMetricService _bodyMetricSvc;

    [ObservableProperty]
    private IEnumerable<BodyMetric> _history = [];

    [ObservableProperty]
    private BodyMetric? _latest;

    [ObservableProperty]
    private double? _weightChange;

    [ObservableProperty]
    private double _newWeightKg;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    private const int UserId = 1;

    public BodyMetricsViewModel(BodyMetricService bodyMetricSvc)
    {
        _bodyMetricSvc = bodyMetricSvc;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        History = await _bodyMetricSvc.GetHistoryAsync(UserId);
        Latest = await _bodyMetricSvc.GetLatestAsync(UserId);
        WeightChange = await _bodyMetricSvc.GetTotalWeightChangeAsync(UserId);
        IsBusy = false;
    }

    [RelayCommand]
    public async Task LogWeightAsync()
    {
        if (NewWeightKg <= 0) return;

        IsBusy = true;
        await _bodyMetricSvc.LogBodyMetricAsync(new BodyMetric
        {
            UserId = UserId,
            WeightKg = NewWeightKg,
            MeasuredAt = DateTime.UtcNow
        });
        StatusMessage = $"Weight {NewWeightKg:F1} kg logged!";
        NewWeightKg = 0;
        await LoadAsync();
        IsBusy = false;
    }
}
