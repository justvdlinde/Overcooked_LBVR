using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils.Core.Events;

public class OperatorUIMenu : MonoBehaviour
{
    public bool IsOpen { get; protected set; } = false;

    [SerializeField] protected TextMeshProUGUI gameModeLabel = null;
    [SerializeField] protected TextMeshProUGUI timerLabel = null;
    [SerializeField] protected Button startGameButton = null;
    [SerializeField] protected Button stopGameButton = null;
    [SerializeField] protected Button replayGameButton = null;

    private GameMode CurrentGameMode => gameModeService.CurrentGameMode;
    private GameModeService gameModeService;
    private GlobalEventDispatcher globalEventDispatcher;


    public void InjectDependencies(GameModeService gameModeService, GlobalEventDispatcher globalEventDispatcher)
    {
        this.gameModeService = gameModeService;
        this.globalEventDispatcher = globalEventDispatcher;
    }

    private void Start()
    {
        if(gameModeService.CurrentGameMode != null)
            gameModeLabel.text = CurrentGameMode.Name;

        globalEventDispatcher.Subscribe<GameModePhaseChangedEvent>(OnGamePhaseChangedEvent);
        globalEventDispatcher.Subscribe<GameModeChangedEvent>(OnGameModeChangedEvent);

        // TODO: !canvasgroup.interactable when not connected to network
    }

    private void OnDestroy()
    {
        globalEventDispatcher.Unsubscribe<GameModePhaseChangedEvent>(OnGamePhaseChangedEvent);
        globalEventDispatcher.Unsubscribe<GameModeChangedEvent>(OnGameModeChangedEvent);
    }

    private void OnEnable()
    {
        startGameButton.onClick.AddListener(OnStartGameButtonPressedEvent);
        stopGameButton.onClick.AddListener(OnStopGameButtonPressedEvent);
        replayGameButton.onClick.AddListener(OnReplayGameButtonPressedEvent);
    }


    private void OnDisable()
    {
        startGameButton.onClick.AddListener(OnStartGameButtonPressedEvent);
        stopGameButton.onClick.AddListener(OnStopGameButtonPressedEvent);
        replayGameButton.onClick.AddListener(OnReplayGameButtonPressedEvent);
    }

    //private void Update()
    //{
    //    if(CurrentGameMode != null)
    //        timerLabel.text = CurrentGameMode.GetTimeReadableString();
    //}

    private void OnGameModeChangedEvent(GameModeChangedEvent @event)
    {
        gameModeLabel.text = @event.GameMode.Name;
    }

    private void OnGamePhaseChangedEvent(GameModePhaseChangedEvent @event)
    {
        startGameButton.gameObject.SetActive(@event.Phase == MatchPhase.PreGame);
        stopGameButton.gameObject.SetActive(@event.Phase == MatchPhase.Active);
        replayGameButton.gameObject.SetActive(@event.Phase == MatchPhase.PostGame);
    }

    private void OnStartGameButtonPressedEvent()
    {
        CurrentGameMode.StartActiveGame();
    }

    private void OnStopGameButtonPressedEvent()
    {
        CurrentGameMode.EndGame();
    }

    private void OnReplayGameButtonPressedEvent()
    {
        CurrentGameMode.Replay();
    }

    public void Open()
    {
        // play open animation
        gameObject.SetActive(true);
        IsOpen = true;
    }

    public void Close()
    {
        // play close animation
        gameObject.SetActive(false);
        IsOpen = false;
    }
}
