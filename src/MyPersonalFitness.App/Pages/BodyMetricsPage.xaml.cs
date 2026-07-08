using MyPersonalFitness.App.ViewModels;

namespace MyPersonalFitness.App;

public partial class BodyMetricsPage : ContentPage
{
    private readonly BodyMetricsViewModel _vm;

    public BodyMetricsPage(BodyMetricsViewModel vm)
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
