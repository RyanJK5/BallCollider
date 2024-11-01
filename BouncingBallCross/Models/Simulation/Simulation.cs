using BouncingBall.Models.Physics;
using Plugin.Maui.Audio;

namespace BouncingBall.Models.Simulation;

public class Simulation : IDrawable
{
    private static readonly Simulation _sim = new();

    public static Simulation GetSimulation() => _sim;

    public bool RestartRequired;

    private readonly Random _random;

    private int _hue;
    private float _newBallPercent;
    private bool _firstDraw = true;

    public readonly SimulationEvent[] SimulationEvents;
    public readonly Rule[] InitialRules;
    public readonly BooleanRule[] DrawRules;
    public readonly BooleanRule[] InitialBoolRules;

    public string AudioSet
    {
        get => Util.AddSpaces(_audioSet.Type.ToString());
        set
        {
            var newSet = new AudioSet(Enum.GetValues<AudioSetType>().First(type => type.ToString().Equals(Util.RemoveSpaces(value)))) 
                ?? throw new ArgumentException("string does not match any known audiosettypes");
            
            if (newSet.Type != _audioSet.Type)
            {
                RestartRequired = true;
            }
            _audioSet = newSet;
        }
    }
    private AudioSet _audioSet;
    private CustomAudioHandler? _sfxHandler;
    private CustomAudioHandler? _songHandler;
    public AudioType AudioType;
    public bool AudioOnOuter { get; set; }
    public bool AudioOnInner { get; set;  }

    public bool NotNone => _audioSet.Type != AudioSetType.None;

    private Circle _container;
    private readonly List<IAudioPlayer> _audioPlayers;
    private readonly List<Ball> _balls;
    private readonly Dictionary<Ball, List<Physics.Point>> _collisionPoints;
    private readonly Dictionary<Ball, List<Ball>> _trailPoints;

    private SimulationEvent this[SimulationEventType type]
    {
        get => SimulationEvents[(int)type];
    }

    private Rule this[RuleType type]
    {
        get => InitialRules[(int)type];
    }

    private BooleanRule this[BooleanRuleType type]
    {
        get => BooleanRule.DrawRule(type) ? DrawRules[(int)type] : InitialBoolRules[(int)type - (int)BooleanRuleType.StaticColor - 1];
    }

    public Simulation()
    {
        _random = new Random();
        _audioPlayers = [];
        _balls = [];
        _collisionPoints = [];
        _trailPoints = [];
        InitialRules = Rule.InitialRules();
        InitialBoolRules = BooleanRule.MiscRules();
        DrawRules = BooleanRule.DrawRules();
        _audioSet = new(AudioSetType.None);
        AudioOnOuter = true;
        AudioType = AudioType.None;

        SimulationEventType[] types = Enum.GetValues<SimulationEventType>();
        SimulationEvents = new SimulationEvent[types.Length];
        for (var i = 0; i < types.Length; i++)
        {
            SimulationEvents[i] = new(types[i]);
        }

        Restart();
    }

    public async void Restart()
    {
        RestartRequired = false;

        _sfxHandler?.StopAll();
        _songHandler?.StopAll();

        foreach (IAudioPlayer player in _audioPlayers)
        {
            player.Dispose();
        }
        _audioPlayers.Clear();

        if (_audioSet is not null)
        {
            foreach (string fileName in _audioSet.AudioFiles)
            {
                _audioPlayers.Add(AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync(fileName)));
            }
        }

        _container = new Circle(175, 175, this[RuleType.ContainerSize].Value);
        _balls.Clear();
        _collisionPoints.Clear();
        _balls.AddRange(CreateBalls((int)MathF.Round(this[RuleType.NumberOfBalls].Value), _container.Center, false));

        _newBallPercent = 0;
    }

    private List<Ball> CreateBalls(int ballCount, Physics.Point center, bool randomDirection)
    {
        float ballRad = this[RuleType.BallSize].Value;
        float radius = (ballCount <= 1) ? 0 : ballRad / MathF.Sin(MathF.PI / ballCount) + 1;
        radius = MathF.Min(radius, _container.Radius - ballRad);

        List<Ball> result = [];
        for (var i = 0; i < ballCount; i++)
        {
            float theta = i * (MathF.Tau / ballCount);

            var newBall = new Ball
            {
                Center = new(
                    radius * MathF.Cos(theta) + center.X,
                    radius * MathF.Sin(theta) + center.Y
                ),
                Radius = this[RuleType.BallSize].Value,
                Number = i + 1,
            };

            if (randomDirection)
            {
                theta = (float)_random.NextDouble() * MathF.Tau;
            }

            newBall.Velocity = (ballCount == 1 ? new Vector2(0, 1) : new Vector2(MathF.Cos(theta), MathF.Sin(theta))) * this[RuleType.InitialSpeed].Value;

            if (this[BooleanRuleType.StaticColor].IsToggled)
            {
                float ballHue = i * (360f / ballCount);
                if (ballCount <= Rule.GetDefaultRange(RuleType.AddBalls).Max)
                {
                    ballHue = _random.Next(360);
                }
                newBall.Color = Util.ColorFromHSV((int)ballHue, Globals.ColorSaturation, Globals.ColorValue);
            }

            result.Add(newBall);
            _trailPoints.Add(newBall, []);
        }
        return result;
    }

    public void Update()
    {
        _hue = (_hue + Globals.ColorHueIncrement) % 360;

        List<Ball> toRemove = [];
        List<Ball> toAdd = [];
        foreach (var ball1 in _balls)
        {
            ball1.Update(_random, this[RuleType.Gravity].Value, this[RuleType.Entropy].Value);

            HandleEventUpdates(SimulationEventType.Constantly, toRemove, toAdd, ball1);

            if (!this[BooleanRuleType.DisableCollisions].IsToggled)
            {
                foreach (var ball2 in _balls)
                {
                    if (!ball2.CollidesWith(ball1))
                    {
                        continue;
                    }
                    HandleEventUpdates(SimulationEventType.BallOnBall, toRemove, toAdd, ball1, ball2);
                    Ball.OnCollision(ball1, ball2);

                    if (AudioOnInner)
                    {
                        PlaySound();
                    }
                }
            }

            if (ball1.CollidesWithOuter(_container))
            {
                HandleEventUpdates(SimulationEventType.BallOnWall, toRemove, toAdd, ball1);

                if (AudioOnOuter)
                {
                    PlaySound();
                }

                ball1.OnCollision(_container, out Physics.Point collisionPoint);

                if (this[BooleanRuleType.ShowCollisionLines].IsToggled)
                {
                    if (_collisionPoints.TryGetValue(ball1, out List<Physics.Point>? value))
                    {
                        value.Add(collisionPoint);
                    }
                    else
                    {
                        _collisionPoints.Add(ball1, [collisionPoint]);
                    }
                }
            }

            UpdateTrailPoints(ball1);
        }
        _balls.AddRange(toAdd);

        if (_balls.Count > Globals.BallCap)
        {
            for (var i = Globals.BallCap; i < _balls.Count; i++)
            {
                _balls.RemoveAt(i);
                i--;
            }
        }
        _balls.RemoveAll(toRemove.Contains);
    }

    private void UpdateTrailPoints(Ball ball)
    {
        _trailPoints[ball].Add(new Ball
        {
            Center = ball.Center,
            Radius = ball.Radius,
            Color = ball.Color ?? Util.ColorFromHSV(_hue, Globals.ColorSaturation, Globals.ColorValue)
        });
        if (_trailPoints[ball].Count > Globals.TrailLength)
        {
            _trailPoints[ball].RemoveAt(0);
        }
    }

    private void HandleEventUpdates(SimulationEventType type, List<Ball> toRemove, List<Ball> toAdd, params Ball[] affectedBalls)
    {
        foreach (Rule rule in this[type].Rules)
        {
            float value = rule.Value;
            if (value == 0)
            {
                continue;
            }
            if (type == SimulationEventType.Constantly)
            {
                if (rule.Type == RuleType.AddBalls)
                {
                    _newBallPercent += MathF.Round(value) * Globals.TimerDelaySeconds / _balls.Count;
                    if (_newBallPercent >= 1)
                    {
                        _newBallPercent -= 1;
                        toAdd.AddRange(CreateBalls(1, _container.Center, true));
                    }
                    if (_newBallPercent <= -1)
                    {
                        _newBallPercent += 1;
                        toRemove.Add(_balls[_random.Next(_balls.Count)]);
                    }
                    continue;
                }
                value *= Globals.TimerDelaySeconds * 10;
            }

            switch (rule.Type)
            {
                case RuleType.SpeedChange:
                    value *= 0.1f;
                    foreach (Ball ball in affectedBalls)
                    {
                        Vector2 velocityDiff = ball.Velocity.Normalized() * value;
                        if (velocityDiff.Length() > ball.Velocity.Length() && value < 0)
                        {
                            ball.Velocity = Vector2.Zero;
                            break;
                        }
                        ball.Velocity += velocityDiff;
                    }
                    break;
                case RuleType.BallSizeChange:
                    foreach (Ball ball in affectedBalls)
                    {
                        ball.Radius += value;
                        if (ball.Radius <= 0)
                        {
                            toRemove.Add(ball);
                        }
                    }
                    break;
                case RuleType.ContainerSizeChange:
                    _container.Radius = MathF.Max(1, _container.Radius + value);
                    break;
                case RuleType.AddBalls:
                    value = MathF.Round(value);
                    if (value >= 1)
                    {
                        toAdd.AddRange(CreateBalls((int)value, _container.Center, value == 1));
                    }
                    if (value <= -1)
                    {
                        int endIndex = (int)MathF.Abs(value);
                        int extras = Math.Min(endIndex - affectedBalls.Length, _balls.Count - toRemove.Count);
                        
                        endIndex = Math.Min(endIndex, affectedBalls.Length);

                        toRemove.AddRange(affectedBalls[0..endIndex]);
                        while (extras > 0)
                        {
                            Ball? ball = _balls.Find(ball => !toRemove.Contains(ball));
                            if (ball is null)
                            {
                                break;
                            }

                            toRemove.Add(ball);
                            extras -= 1;
                        }
                    }

                    break;
                default:
                    throw new ArgumentException("rule must be dynamic");
            }

            foreach (Ball toAddBall in toAdd)
            {
                foreach (Ball existingBall in _balls)
                {
                    if (toAddBall.CollidesWith(existingBall))
                    {
                        Ball.OnCollision(existingBall, toAddBall);
                    }
                }
            }
        }
    }

    private void PlaySound()
    {
        if (AudioType == AudioType.AudioSet && _audioPlayers.Count > 0)
        {
            int index = _random.Next(0, _audioPlayers.Count);
            _audioPlayers[index].Play();
        }
        else if (AudioType == AudioType.Custom)
        {
            _sfxHandler?.PlaySound(AudioType);
        }
        else if (AudioType == AudioType.Song)
        {
            _songHandler?.PlaySound(AudioType);
        }
    }

    public void CreateSoundHandler(Stream stream) 
    {
        _sfxHandler?.Dispose();
        _sfxHandler = new(stream, AudioType);
    }

    public void CreateSongHandler(Stream stream)
    {
        _songHandler?.Dispose();
        _songHandler = new(stream, AudioType);
    }

    public void EndCustomAudio()
    {
        _sfxHandler?.StopAll();
        _songHandler?.StopAll();
    }

    public void RemoveCustomAudio()
    {
        EndCustomAudio();
        _sfxHandler?.Dispose();
        _sfxHandler = null;
        _songHandler?.Dispose();
        _songHandler = null;
    }


    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (_firstDraw)
        {
            RestartRequired = false;
        }
        _firstDraw = false;

        Color color = Util.ColorFromHSV(_hue, Globals.ColorSaturation, Globals.ColorValue);

        foreach (Ball ball in _balls)
        {
            if (this[BooleanRuleType.ShowTrail].IsToggled)
            {
                float alpha = 0.5f;
                for (var i = _trailPoints[ball].Count - 1; i >= 0; i--)
                {
                    alpha -= 0.5f / (_trailPoints[ball].Count + 1);
                    canvas.Alpha = alpha;
                    DrawBall(canvas, _trailPoints[ball][i], color, false);
                }
            }

            canvas.Alpha = 1f;
            DrawBall(canvas, ball, color, this[BooleanRuleType.ShowLine].IsToggled);
        }

        canvas.StrokeSize = 5;
        canvas.StrokeColor = color;
        canvas.DrawCircle(_container.Center.X, _container.Center.Y, _container.Radius);

        canvas.FontColor = Colors.White;
        canvas.FontSize = 18;

        if (RestartRequired)
        {
            canvas.DrawString("Restart Required", 0, 5, 350, 345, HorizontalAlignment.Right, VerticalAlignment.Top);
        }
    }

    private void DrawBall(ICanvas canvas, Ball ball, Color color, bool drawLine)
    {
        if (this[BooleanRuleType.ShowCollisionLines].IsToggled)
        {
            canvas.StrokeSize = 2;
            canvas.StrokeColor = Colors.Gray;

            _collisionPoints.TryGetValue(ball, out List<Physics.Point>? value);
            if (value is not null)
            {
                foreach (Physics.Point point in value)
                {
                    canvas.DrawLine(point.X, point.Y, ball.Center.X, ball.Center.Y);
                }
            }
        }

        canvas.FillColor = ball.Color ?? color;

        canvas.FillCircle(ball.Center.X, ball.Center.Y, ball.Radius);


        if (this[BooleanRuleType.ShowBorder].IsToggled)
        {
            canvas.StrokeSize = 2;
            canvas.StrokeColor = Colors.White;
            canvas.DrawCircle(ball.Center.X, ball.Center.Y, ball.Radius);
        }
        if (this[BooleanRuleType.ShowLine].IsToggled && drawLine)
        {
            float xDist = _container.Center.X - ball.Center.X;
            float radius = _container.Radius;
            float yDist = MathF.Sqrt(radius * radius - xDist * xDist);
            float bottomY = _container.Center.Y + yDist;

            canvas.StrokeSize = 2;
            canvas.StrokeColor = Colors.White;
            canvas.DrawLine(new(ball.Center.X, ball.Center.Y + ball.Radius), new(ball.Center.X, bottomY));
        }
    }
}
