using BouncingBall.Models.Simulation;

namespace BouncingBall.Views;

public partial class DynamicRulesPage : ContentPage
{
	public DynamicRulesPage()
    {
        InitializeComponent();

        Simulation sim = Simulation.GetSimulation();
        BallView.Drawable = sim;
        RuleAccordions.ItemsSource = sim.SimulationEvents;
    }

    private void RestartButton_Pressed(object sender, EventArgs e)
    {
        Simulation.GetSimulation().Restart();
    }

    private void ResetAll_Pressed(object sender, EventArgs e)
    {
        Simulation sim = Simulation.GetSimulation();
        foreach (SimulationEvent evt in sim.SimulationEvents)
        {
            foreach (Rule rule in evt.Rules)
            {
                rule.ResetToDefault.Execute(null);
            }
        }
    }

    private void RuleSlider_Loaded(object sender, EventArgs e)
    {
        if (sender is Slider slider)
        {
            Rule rule = (Rule)slider.BindingContext;
            rule.SetSlider(slider);
        }
    }
}