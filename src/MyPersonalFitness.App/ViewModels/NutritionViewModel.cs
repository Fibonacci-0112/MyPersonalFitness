using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyPersonalFitness.Core.Models;
using MyPersonalFitness.Core.Services;

namespace MyPersonalFitness.App.ViewModels;

public partial class NutritionViewModel : ObservableObject
{
    private readonly NutritionService _nutritionSvc;

    [ObservableProperty]
    private IEnumerable<Meal> _todayMeals = [];

    [ObservableProperty]
    private DailySummary _summary = new();

    [ObservableProperty]
    private bool _isBusy;

    private const int UserId = 1;

    public NutritionViewModel(NutritionService nutritionSvc)
    {
        _nutritionSvc = nutritionSvc;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        TodayMeals = await _nutritionSvc.GetMealsForDayAsync(UserId, DateTime.Today);
        Summary = await _nutritionSvc.GetDailySummaryAsync(UserId, DateTime.Today);
        IsBusy = false;
    }

    [RelayCommand]
    public async Task LogQuickMealAsync(string foodDescription)
    {
        var meal = new Meal
        {
            UserId = UserId,
            MealType = MealType.Other,
            Notes = foodDescription
        };
        await _nutritionSvc.LogMealAsync(meal);
        await LoadAsync();
    }
}
