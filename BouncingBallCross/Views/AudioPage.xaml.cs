using BouncingBall.Models.Simulation;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Storage;
using Microsoft.VisualBasic.FileIO;
using Plugin.Maui.Audio;

namespace BouncingBall.Views;

public partial class AudioPage : ContentPage
{
    private PickOptions _soundOptions;
    private PickOptions _songOptions;


    public AudioPage()
	{
        InitializeComponent();

        Simulation sim = Simulation.GetSimulation();
        BallView.Drawable = sim;

        AudioSetType[] types = Enum.GetValues<AudioSetType>();
        string[] strings = new string[types.Length];
        for (var i = 0; i < strings.Length; i++)
        {
            strings[i] = Util.AddSpaces(types[i].ToString());
        }

        AudioTypeSelectors.ItemsSource = strings;
        AudioTypeSelectors.BindingContext = sim;
        OnWallBox.BindingContext = sim;
        OnBallBox.BindingContext = sim;

        var soundTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>()
        {
            { DevicePlatform.iOS, [ "public.audio" ] },
            { DevicePlatform.Android, [ "audio/*" ] },
            { DevicePlatform.WinUI, [ ".mp3", ".wav", ".mid", ".midi" ] }
        });
        var songTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>()
        {
            { DevicePlatform.Android, [ "audio/midi", "audio/x-midi" ] },
            { DevicePlatform.WinUI, [ ".mid", ".midi"] }
        });

        _soundOptions = new PickOptions()
        {
            PickerTitle = "Select an audio file",
            FileTypes = soundTypes
        };
        _songOptions = new PickOptions()
        {
            PickerTitle = "Select a MIDI file",
            FileTypes = songTypes
        };
    }

    private void RestartButton_Pressed(object sender, EventArgs e)
    {
        Simulation.GetSimulation().Restart();
    }

    private void ResetAll_Pressed(object sender, EventArgs e)
    {
        AudioTypeSelectors.SelectedIndex = (int) AudioSetType.None;

        Simulation sim = Simulation.GetSimulation();
        sim.AudioOnInner = false;
        sim.AudioOnOuter = true;
        sim.AudioType = AudioType.None;
        sim.RemoveCustomAudio();

        AudioSet.IsChecked = false;
        CustomAudio.IsChecked = false;
        CustomSong.IsChecked = false;

        AudioFileName.Text = "";
        SongFileName.Text = "";

        OnWallBox.IsChecked = true;
        OnBallBox.IsChecked = false;
    }

    private void CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        var sim = Simulation.GetSimulation();
        sim.EndCustomAudio();
        if (AudioSet.IsChecked)
        {
            sim.AudioType = AudioType.AudioSet;
            return;
        }
        if (CustomAudio.IsChecked)
        {
            sim.AudioType = AudioType.Custom;
            return;
        }
        if (CustomSong.IsChecked)
        {
            sim.AudioType = AudioType.Song;
            return;
        }
        sim.AudioType = AudioType.None;
    }

    private async void SoundButton_Pressed(object sender, EventArgs e)
    {
        var result = await FilePicker.Default.PickAsync(_soundOptions);
        if (result is not null)
        {
            AudioFileName.Text = result.FileName;
            using var stream = await result.OpenReadAsync();
            Simulation.GetSimulation().CreateSoundHandler(stream);
        }
    }


    private async void SongButton_Pressed(object sender, EventArgs e)
    {
        var result = await FilePicker.Default.PickAsync(_songOptions);
        if (result is not null)
        {
            SongFileName.Text = result.FileName;
            using var stream = await result.OpenReadAsync();
            Simulation.GetSimulation().CreateSongHandler(stream);
        }
    }
}