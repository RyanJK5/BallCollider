namespace BouncingBall.Models.Simulation;

public class SimulationEvent(SimulationEventType type) : SimulationModel<SimulationEventType>(type)
{
    public Rule[] Rules { get; set; } = Rule.DynamicsRules();
}
