using Photon.Pun;
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

    public void Open() 
    {
        currentGameMode = gameModeService.CurrentGameMode;
        if (currentGameMode != null)
            settingsScoreText = currentGameMode.MatchDuration.ToString();
    }

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
            GUILayout.Space(5);
            DrawStoryGamemodeOrders();
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
        GUI.enabled = Photon.Pun.PhotonNetwork.IsMasterClient;

        GUILayout.BeginHorizontal();
        GUILayout.Label("Match Duration: ");
        settingsDurationText = GUILayout.TextField(settingsDurationText);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Update settings"))
        {
            if (int.TryParse(settingsDurationText, out int result))
                currentGameMode.SetMatchDuration(result);
        }

        GUI.enabled = true;
        GUILayout.EndVertical();
    }

    private void DrawCurrentGamemodeOptionsPanel(bool drawDeveloperOptions)
    {
        GUILayout.BeginVertical(currentGameMode.Name, "window");

        GUILayout.Label("Phase: " + currentGameMode.MatchPhase);
        GUILayout.Label("Start requirements met: " + currentGameMode.StartRequirementsAreMet());
        GUILayout.Label("Duration: " + $"{(int)currentGameMode.GameTimer.Duration / 60}:{currentGameMode.GameTimer.Duration % 60:00}");
        GUILayout.Label("Time remaining: " + $"{(int)currentGameMode.GameTimer.TimeRemaining / 60}:{currentGameMode.GameTimer.TimeRemaining % 60:00}");
        GUILayout.Label("timer is running: " + currentGameMode.GameTimer.IsRunning);

        if (drawDeveloperOptions)
        {
            GUI.enabled = currentGameMode.MatchPhase == MatchPhase.PreGame;
            if (PhotonNetwork.IsMasterClient)
            {
                if (GUILayout.Button("StartGame"))
                    currentGameMode.StartActiveGame();
            }
            else
            {
                if (GUILayout.Button("StartGame"))
                    currentGameMode.AttemptToStartActiveGame();
            }
            if (PhotonNetwork.IsMasterClient)
            {
                GUI.enabled = currentGameMode.MatchPhase == MatchPhase.Active;
                if (GUILayout.Button("EndGame"))
                    currentGameMode.EndGame();

                GUI.enabled = currentGameMode.MatchPhase == MatchPhase.PostGame;
                if (GUILayout.Button("Replay"))
                    currentGameMode.Replay();
            }
            GUI.enabled = true;

            if (currentGameMode.MatchPhase == MatchPhase.Active)
            {
                GUILayout.Space(10);
                if (currentGameMode.GameTimer.IsRunning)
                {
                    if (GUILayout.Button("Pause"))
                        currentGameMode.GameTimer.Stop();
                }
                else
                {
                    if (GUILayout.Button("Resume"))
                        currentGameMode.GameTimer.Resume();
                }
            }
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
        GUILayout.Label("Finished orders: " + scoreboard.OrdersCount);
        GUILayout.Label("Deliverd orders: " + scoreboard.DeliveredOrdersCount);
        GUILayout.Label("Timer exceeded orders: " + scoreboard.TimerExceededOrdersCount);
        GUILayout.EndVertical();
    }

    private void DrawStoryGamemodeOrders()
    {
        StoryMode story = gameModeService.CurrentGameMode as StoryMode;
        if (story == null)
            return;
        TieredOrderGenerator orderGenerator = story.OrderGenerator;

        GUILayout.BeginVertical(currentGameMode.Name + " Orders", "window");
        GUILayout.BeginHorizontal();
        GUILayout.Label("Current Tier: " + orderGenerator.currentTier);
        if (GUILayout.Button("+"))
            orderGenerator.currentTier++;
        if (GUILayout.Button("-"))
            orderGenerator.currentTier--;
        GUILayout.EndHorizontal();
        GUILayout.Label("Completed orders in succession: " + orderGenerator.completedOrdersInSuccession);

        if (GUILayout.Button("Add new order"))
        {
            if (SceneOrderDeliveryManager.HasFreeDisplay())
                story.CreateNewActiveOrder(SceneOrderDeliveryManager.GetFreeDisplay().OrderNumber);
            else
                Debug.LogWarning("No Free Display available for new order");
        }

        for (int i = 0; i < story.OrdersController.ActiveOrders.Length; i++)
        {
            Order order = story.OrdersController.ActiveOrders[i];
            if (order == null)
            {
                GUILayout.Label("#" + i + ": null");
                continue;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(order.orderIndex + ": " + order + " (" + order.timer.TimeRemaining + ")");

            if (order.timer.IsRunning)
            {
                if (GUILayout.Button("Pause"))
                    order.timer.Stop();
            }
            else
            {
                 if (GUILayout.Button("Resume"))
                    order.timer.Resume();
            }

            if (GUILayout.Button("Deliver"))
                story.CheatDeliverDish(order.orderIndex);
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }
}
