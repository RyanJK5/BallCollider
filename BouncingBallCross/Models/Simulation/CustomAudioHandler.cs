using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Plugin.Maui.Audio;

namespace BouncingBall.Models.Simulation;

public class CustomAudioHandler
{
    private readonly SortedDictionary<double, double>? _timesToDurations;
    private int _position;

    private readonly TempoMap? _map;
    private readonly ICollection<TimedEvent>? _events;

    private readonly IAudioPlayer[] _players;

    private readonly AudioType _type;

    public CustomAudioHandler(Stream stream, AudioType type)
    {
        _type = type;
        if (type == AudioType.Song)
        {
            MidiFile file = MidiFile.Read(stream);
            _events = file.GetTimedEvents();
            _map = file.GetTempoMap();

            // Comparer<double>.Create((a, b) => -a.CompareTo(b))
            _timesToDurations = new SortedDictionary<double, double>();
        }

        _players = new IAudioPlayer[Globals.MaxSimultaneousCustomAudios];
        for (int i = 0; i < _players.Length; i++)
        {
            stream.Position = 0;
            _players[i] = AudioManager.Current.CreatePlayer(stream);
        }
        Restart();
    }

    public void Restart()
    {
        List<TimedEvent> singlePairs = [];

        _position = 0;
        if (_timesToDurations is null || _events is null)
        {
            return;
        }

        _timesToDurations.Clear();
        foreach (var evt in _events)
        {
            double time = evt.TimeAs<MetricTimeSpan>(_map).TotalSeconds;
            if (evt.Event is NoteOnEvent && !_timesToDurations.ContainsKey(time))
            {
                singlePairs.Add(evt);
            }
            else if (evt.Event is NoteOffEvent off)
            {
                TimedEvent? onEvt = singlePairs.Find(on => ((NoteEvent)on.Event).NoteNumber == off.NoteNumber);
                if (onEvt is not null)
                {
                    singlePairs.Remove(onEvt);

                    double onTime = onEvt.TimeAs<MetricTimeSpan>(_map).TotalSeconds;
                    _timesToDurations.TryAdd(onTime, time - onTime);
                }
            }
        }
    }

    public void PlaySound(AudioType type)
    {
        if (type != _type)
        {
            return;
        }
        if (_timesToDurations is not null)
        {
            PlayNextNote();
            return;
        }

        IAudioPlayer? player = SelectPlayer();
        player?.Play();
    }

    public void Dispose()
    {
        foreach (IAudioPlayer player in _players)
        {
            player.Dispose();
        }
    }

    public void StopAll()
    {
        _position = 0;
        foreach (IAudioPlayer player in _players)
        {
            player.Stop();
        }
    }

    private void PlayNextNote()
    {
        if (_timesToDurations is null)
        {
            return;
        }

        IAudioPlayer? player = SelectPlayer();
        if (player is null)
        {
            return;
        }

        double key = _timesToDurations.Keys.ToArray()[_position];

        player.Seek(key);
        player.Play();

        if (_position == _timesToDurations.Count - 1)
        {
            return;
        }

        var task = new Task(() =>
        {
            Thread.Sleep((int) (_timesToDurations[key] * 1000));
            player.Pause();
        });
        task.Start();

        _position++;
    }

    private IAudioPlayer? SelectPlayer()
    {
        double latestPosition = 0;
        IAudioPlayer? latestPlayer = null;
        foreach (IAudioPlayer player in _players)
        {
            if (player.CurrentPosition > latestPosition)
            {
                latestPlayer = player;
                latestPosition = player.CurrentPosition;
            }
            if (!player.IsPlaying)
            {
                return player;
            }
        }
        return latestPlayer;
    }
}
