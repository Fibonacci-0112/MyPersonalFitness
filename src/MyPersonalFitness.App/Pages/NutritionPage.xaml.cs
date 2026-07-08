using MyPersonalFitness.App.ViewModels;

namespace MyPersonalFitness.App;

public partial class NutritionPage : ContentPage
{
    private readonly NutritionViewModel _vm;

    public NutritionPage(NutritionViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCommand.ExecuteAsync(null);
    }
}
