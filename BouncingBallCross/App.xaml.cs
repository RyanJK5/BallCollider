using BouncingBall.Models.Simulation;
using BouncingBall.Views;

namespace BouncingBall;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new MainPage();
    }

    protected override Window CreateWindow(IActivationState? state)
    {
        Window window = base.CreateWindow(state);

        window.Stopped += (s, e) =>
        {
            Globals.Deactivated = true;
            Simulation.GetSimulation().EndCustomAudio();
        };
        window.Resumed += (s, e) =>
        {
            Globals.Deactivated = false;
        };

        return window;
    }

}
