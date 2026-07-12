using MyPersonalFitness.App.ViewModels;

namespace MyPersonalFitness.App;

public partial class WorkoutsPage : ContentPage
{
    private readonly WorkoutsViewModel _vm;

    public WorkoutsPage(WorkoutsViewModel vm)
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
