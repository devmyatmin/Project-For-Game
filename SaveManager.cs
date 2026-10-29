using Godot;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

public partial class SaveManager : Node
{
    private const string SaveFileName = "survival_save.json";
    private const int CurrentSaveVersion = 1;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public static SaveManager Instance { get; private set; }

    [Signal]
    public delegate void SaveCompletedEventHandler();

    [Signal]
    public delegate void LoadCompletedEventHandler();

    [Signal]
    public delegate void SaveFailedEventHandler(string message);

    [Signal]
    public delegate void LoadFailedEventHandler(string message);

    public GameSaveData CurrentData { get; private set; } = new();

    private string SavePath =>
        "user://".PathJoin(SaveFileName);

    public override void _EnterTree()
    {
        if (Instance != null && Instance != this)
        {
            GD.PushError("Duplicate SaveManager instance detected.");
            return;
        }

        Instance = this;
    }

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }

    public void SetCurrentData(GameSaveData data)
    {
        if (data == null)
        {
            GD.PushError("Cannot assign null save data.");
            return;
        }

        CurrentData = data;
    }

    public Error SaveGame()
    {
        try
        {
            CurrentData.SaveVersion = CurrentSaveVersion;
            CurrentData.SavedAtUnix =
                DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            string json = JsonSerializer.Serialize(
                CurrentData, JsonOptions);

            using var file = FileAccess.Open(
                SavePath, FileAccess.ModeFlags.Write);

            if (file == null)
            {
                Error error = FileAccess.GetOpenError();
                EmitSignal(
                    SignalName.SaveFailed,
                    $"Could not open save file: {error}");
                return error;
            }

            file.StoreString(json);

            Error writeError = file.GetError();

            if (writeError != Error.Ok)
            {
                EmitSignal(
                    SignalName.SaveFailed,
                    $"Could not write save file: {writeError}");
                return writeError;
            }

            EmitSignal(SignalName.SaveCompleted);
            return Error.Ok;
        }
        catch (Exception ex)
        {
            GD.PushError($"Save failed: {ex.Message}");
            EmitSignal(SignalName.SaveFailed, ex.Message);
            return Error.Failed;
        }
    }

    public Error LoadGame()
    {
        if (!FileAccess.FileExists(SavePath))
        {
            EmitSignal(
                SignalName.LoadFailed,
                "No save file exists.");
            return Error.FileNotFound;
        }

        try
        {
            using var file = FileAccess.Open(
                SavePath, FileAccess.ModeFlags.Read);

            if (file == null)
            {
                Error error = FileAccess.GetOpenError();
                EmitSignal(
                    SignalName.LoadFailed,
                    $"Could not open save file: {error}");
                return error;
            }

            string json = file.GetAsText();

            GameSaveData loadedData =
                JsonSerializer.Deserialize<GameSaveData>(
                    json, JsonOptions);

            if (loadedData == null)
            {
                EmitSignal(
                    SignalName.LoadFailed,
                    "Save data is empty or invalid.");
                return Error.FileCorrupt;
            }

            if (loadedData.SaveVersion != CurrentSaveVersion)
            {
                EmitSignal(
                    SignalName.LoadFailed,
                    $"Unsupported save version: {loadedData.SaveVersion}");
                return Error.FileCorrupt;
            }

            CurrentData = loadedData;

            EmitSignal(SignalName.LoadCompleted);
            return Error.Ok;
        }
        catch (JsonException ex)
        {
            GD.PushError($"Invalid save JSON: {ex.Message}");
            EmitSignal(SignalName.LoadFailed, ex.Message);
            return Error.FileCorrupt;
        }
        catch (Exception ex)
        {
            GD.PushError($"Load failed: {ex.Message}");
            EmitSignal(SignalName.LoadFailed, ex.Message);
            return Error.Failed;
        }
    }

    public bool HasSave()
    {
        return FileAccess.FileExists(SavePath);
    }

    public Error DeleteSave()
    {
        if (!FileAccess.FileExists(SavePath))
            return Error.FileNotFound;

        Error error = DirAccess.RemoveAbsolute(
            ProjectSettings.GlobalizePath(SavePath));

        if (error != Error.Ok)
            GD.PushError($"Could not delete save: {error}");

        return error;
    }

    public void ResetCurrentData()
    {
        CurrentData = new GameSaveData();
    }
}

public sealed class GameSaveData
{
    public int SaveVersion { get; set; } = 1;

    public long SavedAtUnix { get; set; }

    // Player state
    public float Health { get; set; } = 100.0f;
    public float Hunger { get; set; } = 100.0f;
    public float Thirst { get; set; } = 100.0f;

    // Player world position
    public float PlayerX { get; set; }
    public float PlayerY { get; set; }
    public float PlayerZ { get; set; }

    // World state
    public int WorldSeed { get; set; }
    public int DayCount { get; set; } = 1;

    // Inventory and building data can be added
    // as the actual project schema is established.
}

