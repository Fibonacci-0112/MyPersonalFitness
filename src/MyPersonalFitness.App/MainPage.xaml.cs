namespace MyPersonalFitness.App;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnWorkoutsClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//workouts");

    private async void OnNutritionClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//nutrition");

    private async void OnBodyMetricsClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//bodymetrics");

    private async void OnGoalsClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync("//goals");
}
