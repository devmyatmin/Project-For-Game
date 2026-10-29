using Godot;
using System;

public partial class AudioManager : Node
{
    public static AudioManager Instance { get; private set; }

    private AudioStreamPlayer _musicPlayer;
    private AudioStreamPlayer _sfxPlayer;
    private AudioStreamPlayer _ambientPlayer;

    private const string MusicBus = "Music";
    private const string SfxBus = "SFX";
    private const string AmbientBus = "Ambient";

    public override void _EnterTree()
    {
        if (Instance != null && Instance != this)
        {
            GD.PushError("Duplicate AudioManager instance.");
            return;
        }

        Instance = this;
    }

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;

        _musicPlayer = CreatePlayer("MusicPlayer", MusicBus);
        _sfxPlayer = CreatePlayer("SFXPlayer", SfxBus);
        _ambientPlayer = CreatePlayer("AmbientPlayer", AmbientBus);
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }

    private AudioStreamPlayer CreatePlayer(
        string playerName, string busName)
    {
        var player = new AudioStreamPlayer
        {
            Name = playerName,
            Bus = AudioServer.GetBusIndex(busName) >= 0
                ? busName
                : "Master"
        };

        AddChild(player);
        return player;
    }

    public void PlayMusic(AudioStream stream, float volumeDb = 0.0f)
    {
        if (stream == null)
            return;

        if (_musicPlayer.Stream == stream &&
            _musicPlayer.Playing)
            return;

        _musicPlayer.Stream = stream;
        _musicPlayer.VolumeDb = volumeDb;
        _musicPlayer.Play();
    }

    public void StopMusic()
    {
        _musicPlayer?.Stop();
    }

    public void SetMusicVolume(float linearVolume)
    {
        SetBusVolume(MusicBus, linearVolume);
    }

    public void PlaySFX(
        AudioStream stream, float volumeDb = 0.0f)
    {
        if (stream == null)
            return;

        _sfxPlayer.Stream = stream;
        _sfxPlayer.VolumeDb = volumeDb;
        _sfxPlayer.Play();
    }

    public void StopSFX()
    {
        _sfxPlayer?.Stop();
    }

    public void PlayAmbient(
        AudioStream stream, float volumeDb = 0.0f)
    {
        if (stream == null)
            return;

        if (_ambientPlayer.Stream == stream &&
            _ambientPlayer.Playing)
            return;

        _ambientPlayer.Stream = stream;
        _ambientPlayer.VolumeDb = volumeDb;
        _ambientPlayer.Play();
    }

    public void StopAmbient()
    {
        _ambientPlayer?.Stop();
    }

    public void SetSFXVolume(float linearVolume)
    {
        SetBusVolume(SfxBus, linearVolume);
    }

    public void SetAmbientVolume(float linearVolume)
    {
        SetBusVolume(AmbientBus, linearVolume);
    }

    public void StopAll()
    {
        _musicPlayer?.Stop();
        _sfxPlayer?.Stop();
        _ambientPlayer?.Stop();
    }

    public void SetMasterVolume(float linearVolume)
    {
        SetBusVolume("Master", linearVolume);
    }

    private static void SetBusVolume(
        string busName, float linearVolume)
    {
        int busIndex = AudioServer.GetBusIndex(busName);

        if (busIndex < 0)
            return;

        float volume = Mathf.Clamp(linearVolume, 0.0f, 1.0f);

        AudioServer.SetBusVolumeDb(
            busIndex,
            volume <= 0.001f
                ? -80.0f
                : Mathf.LinearToDb(volume));
    }
}
