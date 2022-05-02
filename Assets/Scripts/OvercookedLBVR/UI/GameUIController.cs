using System;
using UnityEngine;
using Utils.Core.Events;
using Utils.Core.Services;

public class GameUIController : MonoBehaviour
{
    [SerializeField] private GamemodeUI gamemodeUI = null;
    [SerializeField] private GameOverUI gameOverUI = null;

    private GlobalEventDispatcher globalEventDispatcher;
    private GameMode gamemode;

    private void Awake()
    {
        globalEventDispatcher = GlobalServiceLocator.Instance.Get<GlobalEventDispatcher>();
    }

    private void Start()
    {
        gamemode = GlobalServiceLocator.Instance.Get<GameModeService>().CurrentGameMode;
        gameOverUI.Hide();

        if (gamemode != null)
        {
            if (gamemode.MatchPhase == MatchPhase.PreGame)
                gamemodeUI.Show(gamemode);
            else
                gamemodeUI.Hide();
        }
        else
        {
            gamemodeUI.Hide();
        }
    }

    private void OnEnable()
    {
        globalEventDispatcher.Subscribe<GameOverEvent>(OnGameOverEvent);
        globalEventDispatcher.Subscribe<GameModeChangedEvent>(OnGamemodeChangedEvent);
        globalEventDispatcher.Subscribe<StartGameEvent>(OnGameStartEvent);
        globalEventDispatcher.Subscribe<ReplayEvent>(OnReplayEvent);
    }

    private void OnDisable()
    {
        globalEventDispatcher.Unsubscribe<GameOverEvent>(OnGameOverEvent);
        globalEventDispatcher.Unsubscribe<GameModeChangedEvent>(OnGamemodeChangedEvent);
        globalEventDispatcher.Subscribe<StartGameEvent>(OnGameStartEvent);
    }

    private void OnGameOverEvent(GameOverEvent @event)
    {
        gameOverUI.Show(@event.GameResult);
    }

    private void OnGamemodeChangedEvent(GameModeChangedEvent @event)
    {
        gamemodeUI.Show(@event.GameMode);
        gameOverUI.Hide();
    }

    private void OnGameStartEvent(StartGameEvent obj)
    {
        gameOverUI.Hide();
        gamemodeUI.Hide();
    }

    private void OnReplayEvent(ReplayEvent @event)
    {
        gameOverUI.Hide();
        gamemodeUI.Show(gamemode);
    }
}
