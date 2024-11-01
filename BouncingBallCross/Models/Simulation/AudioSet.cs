namespace BouncingBall.Models.Simulation;

public class AudioSet
{
    public readonly AudioSetType Type;

    public readonly string[] AudioFiles;

    public string Name => Util.AddSpaces(Type.ToString());

    public AudioSet(AudioSetType type)
    {
        Type = type;
        if (type == AudioSetType.None)
        {
            AudioFiles = [];
            return;
        }

        AudioFiles = ["C2", "D2", "E2", "F2", "G2", "A2", "B2", "C3"];

        for (var i = 0; i < AudioFiles.Length; i++)
        {
            AudioFiles[i] = type.ToString() + "/" + AudioFiles[i] + ".wav";
        }
    }
}
