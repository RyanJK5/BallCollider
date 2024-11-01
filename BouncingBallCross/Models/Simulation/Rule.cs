using BouncingBall.Models.Physics;
using System.Windows.Input;

namespace BouncingBall.Models.Simulation;

public class Rule : SimulationModel<RuleType>
{
    public ICommand ResetToDefault { get; set; }

    private Slider? _slider;

    public void SetSlider(Slider slider)
    {
        _slider = slider;
    }

    private class ResetCommand(Rule parent) : ICommand
    {
        private readonly Rule Parent = parent;

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            if (Parent._slider is not null)
            {
                Parent._slider.Value = GetDefaultValue(Parent.Type);
            }
            Parent.Value = GetDefaultValue(Parent.Type);
        }
    }

    public Rule(RuleType type) : base(type)
    {
        Value = GetDefaultValue(type);
        ResetToDefault = new ResetCommand(this);
    }

    private float _ruleValue;
    public float Value
    {
        get => _ruleValue;
        set {
            if (value == _ruleValue)
            {
                return;
            }

            _ruleValue = MathF.Abs(value) < 0.5f ? 0 : value;
            if (RestartRequired(Type))
            {
                Simulation? sim = Simulation.GetSimulation();
                if (sim is not null)
                {
                    sim.RestartRequired = true;
                }
            }
        }
    }

    public float Minimum => GetDefaultRange(Type).Min;

    public float Maximum => GetDefaultRange(Type).Max;

    public void RestoreDefault() => _ruleValue = GetDefaultValue(Type);

    public static float GetDefaultValue(RuleType rule) =>
        rule switch
        {
            RuleType.NumberOfBalls => 1,
            RuleType.Gravity => 30,
            RuleType.Entropy => 1,
            RuleType.InitialSpeed => 0,
            RuleType.BallSize => 20,
            RuleType.ContainerSize => 150,
            RuleType.BallSizeChange => 0,
            RuleType.ContainerSizeChange => 0,
            RuleType.SpeedChange => 0,
            RuleType.AddBalls => 0,
            _ => throw new ArgumentException("Must provide non-null ruletype")
        }
    ;

    public static Range<float> GetDefaultRange(RuleType rule) =>
        rule switch
        {
            RuleType.NumberOfBalls => new(0, 20),
            RuleType.Gravity => new(0, 100),
            RuleType.Entropy => new(0, 10),
            RuleType.InitialSpeed => new(0, 20),
            RuleType.BallSize => new(1, 50),
            RuleType.ContainerSize => new(1, 200),
            RuleType.BallSizeChange => new(-10, 10),
            RuleType.ContainerSizeChange => new(-10, 10),
            RuleType.SpeedChange => new(-10, 10),
            RuleType.AddBalls => new(-5, 5),
            _ => throw new ArgumentException("Must provide non-null ruletype"),
        }
    ;

    public static bool RestartRequired(RuleType type) => InitialRule(type) && type != RuleType.Gravity && type != RuleType.Entropy;

    public static Rule[] InitialRules()
    {
        List<Rule> result = [];
        RuleType[] types = Enum.GetValues<RuleType>().Where(InitialRule).ToArray();
        foreach (RuleType type in types)
        {
            result.Add(new(type));
        }
        return [.. result];
    }

    public static Rule[] DynamicsRules()
    {
        List<Rule> result = [];
        RuleType[] types = Enum.GetValues<RuleType>().Where(rule => !InitialRule(rule)).ToArray();
        foreach (RuleType type in types)
        {
            result.Add(new(type));
        }
        return [.. result];
    }
    public static bool InitialRule(RuleType rule) =>
        rule == RuleType.Gravity || 
        rule == RuleType.Entropy || 
        rule == RuleType.NumberOfBalls || 
        rule == RuleType.InitialSpeed || 
        rule == RuleType.BallSize ||
        rule == RuleType.ContainerSize
    ;

    public static Rule[] GetRules()
    {
        List<Rule> result = [];
        foreach (RuleType type in Enum.GetValues<RuleType>())
        {
            result.Add(new(type));
        }
        return [.. result];
    }
}