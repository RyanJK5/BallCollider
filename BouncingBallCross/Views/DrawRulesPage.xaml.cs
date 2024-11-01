using BouncingBall.Models.Simulation;

namespace BouncingBall.Views;

public partial class DrawRulesPage : ContentPage
{
	public DrawRulesPage()
	{
		InitializeComponent();

        Simulation sim = Simulation.GetSimulation();
        BallView.Drawable = sim;
        SwitchControls.ItemsSource = sim.DrawRules;
    }

    private void RestartButton_Pressed(object sender, EventArgs e)
    {
        Simulation.GetSimulation().Restart();
    }

    private void ResetAll_Pressed(object sender, EventArgs e)
    {
        Simulation sim = Simulation.GetSimulation();
        foreach (BooleanRule rule in sim.DrawRules)
        {
            rule.IsToggled = false;
        }
    }

    private void Switch_Loaded(object sender, EventArgs e)
    {
        if (sender is Switch swtch)
        {
            BooleanRule rule = (BooleanRule) swtch.BindingContext;
            rule.SetSwitch(swtch);
        }
    }
}