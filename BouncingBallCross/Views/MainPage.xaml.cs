using BouncingBall.Models.Simulation;

namespace BouncingBall.Views;

public partial class MainPage : TabbedPage
{
    public MainPage()
    {
        InitializeComponent();
        Loaded += CreateTimer;
    }

    private void CreateTimer(object? sender, EventArgs e)
    {
        var timer = Application.Current?.Dispatcher.CreateTimer();
        if (timer is null)
        {
            return;
        }

        timer.Interval = TimeSpan.FromMilliseconds(Globals.TimerDelayMilliseconds);
        timer.Tick += (sender, args) => MainThread.BeginInvokeOnMainThread(Update);
        timer.Start();
    }

    private void Update()
    {
        if (Globals.Deactivated)
        {
            return;
        }

        Simulation.GetSimulation().Update();
        try
        {
            ((GraphicsView)CurrentPage.FindByName("BallView"))?.Invalidate();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
