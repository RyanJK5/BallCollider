namespace BouncingBall.Models.Simulation;

public class BooleanRule(BooleanRuleType type) : SimulationModel<BooleanRuleType>(type)
{
    private bool _isToggled;
    
    private Switch? _switch;
    public void SetSwitch(Switch swtch)
    {
        _switch = swtch;
    }


    public bool IsToggled 
    { 
        get => _isToggled; 
        set {
            _isToggled = value;
            if (_switch is not null)
            {
                _switch.IsToggled = value;
            }
            if (RestartRequired(type))
            {
                var sim = Simulation.GetSimulation();
                if (sim is not null)
                {
                    sim.RestartRequired = true;
                }
            }
        }
    }

    private bool _isEnabled = true;
    public bool IsEnabled
    {
        get => _isEnabled;
        set => _isEnabled = value;
    }

    public static BooleanRule[] Rules()
    {
        List<BooleanRule> result = [];
        foreach (BooleanRuleType type in Enum.GetValues<BooleanRuleType>())
        {
            result.Add(new(type));
        }
        return [.. result];
    }

    public static BooleanRule[] DrawRules()
    {
        List<BooleanRule> result = [];
        foreach (BooleanRuleType type in Enum.GetValues<BooleanRuleType>())
        {
            if (DrawRule(type))
            {
                result.Add(new(type));
            }
        }
        return [.. result];
    }

    public static BooleanRule[] MiscRules()
    {
        List<BooleanRule> result = [];
        foreach (BooleanRuleType type in Enum.GetValues<BooleanRuleType>())
        {
            if (!DrawRule(type))
            {
                result.Add(new(type));
            }
        }
        return [.. result];
    }

    public static bool DrawRule(BooleanRuleType type) =>
        type == BooleanRuleType.ShowTrail ||
        type == BooleanRuleType.ShowLine ||
        type == BooleanRuleType.ShowBorder ||
        type == BooleanRuleType.ShowCollisionLines ||
        type == BooleanRuleType.StaticColor;

    public static bool RestartRequired(BooleanRuleType type) => type == BooleanRuleType.StaticColor;
}
