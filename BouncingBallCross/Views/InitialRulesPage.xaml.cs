using BouncingBall.Models.Simulation;

namespace BouncingBall.Views;

public partial class InitialRulesPage : ContentPage
{
	public InitialRulesPage()
	{
		InitializeComponent();

        Simulation sim = Simulation.GetSimulation();
        BallView.Drawable = sim;
        SliderControls.ItemsSource = sim.InitialRules;
        SwitchControls.ItemsSource = sim.InitialBoolRules;
    }

    private void RestartButton_Pressed(object sender, EventArgs e)
    {
        Simulation.GetSimulation().Restart();
    }

    private void ResetAll_Pressed(object sender, EventArgs e)
    {
        Simulation sim = Simulation.GetSimulation();
        foreach (Rule rule in sim.InitialRules)
        {
            rule.ResetToDefault.Execute(null);
        }
    }

    private void RuleSlider_Loaded(object sender, EventArgs e)
    {
        if (sender is Slider slider)
        {
            Rule rule = (Rule) slider.BindingContext;
            rule.SetSlider(slider);
        }
    }

    private void Switch_Loaded(object sender, EventArgs e)
    {
        if (sender is Switch swtch)
        {
            BooleanRule rule = (BooleanRule)swtch.BindingContext;
            rule.SetSwitch(swtch);
        }
    }
}