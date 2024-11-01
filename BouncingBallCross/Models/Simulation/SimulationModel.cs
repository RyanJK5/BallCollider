
namespace BouncingBall.Models.Simulation;

public class SimulationModel<T>(T type) where T : Enum
{
    public readonly T Type = type;

    public string Name
    {
        get => Util.AddSpaces(Type.ToString());
    }

    public string UnspacedName
    {
        get => Type.ToString();
    }
}
