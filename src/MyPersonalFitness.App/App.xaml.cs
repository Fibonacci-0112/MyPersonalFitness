namespace MyPersonalFitness.App;

public partial class App : Application
{
    public App(MainPage mainPage)
    {
        InitializeComponent();
        MainPage = new NavigationPage(mainPage)
        {
            BarBackgroundColor = Color.FromArgb("#512BD4"),
            BarTextColor = Colors.White
        };
    }
}
