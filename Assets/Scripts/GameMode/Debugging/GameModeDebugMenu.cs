using System;
using UnityEngine;

public class GameModeDebugMenu : IDebugMenu
{
    private readonly GameModeService gameModeService;
    private Vector2 scrollPosition;
    private GameMode currentGameMode;

    private string[] gamemodeStrings;
    private int currentGamemodeIndex = -1;

    private string settingsScoreText;
    private string settingsDurationText;

    public GameModeDebugMenu(GameModeService gameModeService)
    {
        this.gameModeService = gameModeService;
        gamemodeStrings = Enum.GetNames(typeof(GameModeEnum));
    }

    public void Open() { }
    public void Close() { }

    public void OnGUI(bool drawDeveloperOptions)
    {
        GUILayout.BeginVertical("box", GUILayout.MinWidth(400));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        currentGameMode = gameModeService.CurrentGameMode;

        DrawInfo(drawDeveloperOptions);
        if (currentGameMode != null)
        {
            GUILayout.Space(5);
            DrawSettingsPanel();
            GUILayout.Space(5);
            DrawCurrentGamemodeOptionsPanel(drawDeveloperOptions);
            GUILayout.Space(5);
            DrawStoryGamemodeInfoPanel();
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void DrawInfo(bool drawDeveloperOptions)
    {
        GUILayout.BeginVertical("Gamemode", "window");
        if (currentGameMode == null)
            GUILayout.Label("Gamemode: " + (currentGameMode != null ? currentGameMode.Name : "-"));

        if (drawDeveloperOptions)
        {
            int prevIndex = currentGamemodeIndex;
            GUILayout.BeginHorizontal();
            GUILayout.Label("New Gamemode: ");
            currentGamemodeIndex = GUILayout.Toolbar(currentGamemodeIndex, gamemodeStrings, GUILayout.MaxWidth(230));
            if (prevIndex != currentGamemodeIndex)
                gameModeService.StartNewGame((GameModeEnum)currentGamemodeIndex);
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
    }

    private void DrawSettingsPanel()
    {
        GUILayout.BeginVertical(currentGameMode.Name + " Settings", "window");

        GUILayout.BeginHorizontal();
        GUILayout.Label("Score Target: ");
        settingsScoreText = GUILayout.TextField(settingsScoreText);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Match Duration: ");
        settingsDurationText = GUILayout.TextField(settingsDurationText);
        GUILayout.EndHorizontal();

        //if (GUILayout.Button("Update settings"))
        //{
        //    GameModeSettings settings = ScriptableObject.CreateInstance<GameModeSettings>();
        //    if (int.TryParse(settingsScoreText, out int result))
        //        settings.scoreTarget = result;
        //    if (int.TryParse(settingsDurationText, out result))
        //        settings.matchDuration = result;
        //    currentGameMode.Setup(settings);
        //}
        GUILayout.EndVertical();
    }

    private void DrawCurrentGamemodeOptionsPanel(bool drawDeveloperOptions)
    {
        GUILayout.BeginVertical(currentGameMode.Name, "window");

        GUILayout.Label("Phase: " + currentGameMode.MatchPhase);
        GUILayout.Label("Start requirements met: " + currentGameMode.StartRequirementsAreMet());
        //GUILayout.Label("Duration: " + string.Format("{0:0:00}", currentGameMode.MatchDuration));
        //GUILayout.Label("Time remaining: " + currentGameMode.GetTimeReadableString());

        if (drawDeveloperOptions)
        {
            if (GUILayout.Button("StartGame"))
                currentGameMode.StartActiveGame();
            if (GUILayout.Button("EndGame"))
                currentGameMode.EndGame();
            if (GUILayout.Button("Replay"))
                currentGameMode.Replay();
        }
        GUILayout.EndVertical();
    }

    // TODO: make this work for all gamemodes, not just storymode
    private void DrawStoryGamemodeInfoPanel()
    {
        StoryMode story = gameModeService.CurrentGameMode as StoryMode;
        if (story == null) 
            return;

        StoryModeScoreboard scoreboard = story.Scoreboard as StoryModeScoreboard;

        GUILayout.BeginVertical(currentGameMode.Name + " Scoreboard", "window");
        GUILayout.Label("Points/Max: " + scoreboard.TotalPoints + "/" + scoreboard.MaxAchievablePoints);
        GUILayout.Label("Finished orders: " + scoreboard.FinishedOrdersCount);
        GUILayout.Label("Deliverd orders: " + scoreboard.DeliveredOrdersCount);
        GUILayout.Label("Timer exceeded orders: " + scoreboard.TimerExceededOrdersCount);
        GUILayout.EndVertical();
    }
}
