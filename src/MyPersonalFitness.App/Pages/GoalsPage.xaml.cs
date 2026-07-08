using MyPersonalFitness.App.ViewModels;

namespace MyPersonalFitness.App;

public partial class GoalsPage : ContentPage
{
    private readonly GoalsViewModel _vm;

    public GoalsPage(GoalsViewModel vm)
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
