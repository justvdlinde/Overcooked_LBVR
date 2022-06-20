using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Events;
using Utils.Core.Services;

public class StartGameButton : MonoBehaviour
{
    [SerializeField] private PhysicsButton button = null;
    [SerializeField] private float delayBetweenEndAndReplay = 2;

    private GameModeService gameModeService;
    private GlobalEventDispatcher eventDispatcher;

    private void Awake()
    {
        gameModeService = GlobalServiceLocator.Instance.Get<GameModeService>();
        eventDispatcher = GlobalServiceLocator.Instance.Get<GlobalEventDispatcher>();
        eventDispatcher.Subscribe<GameModePhaseChangedEvent>(OnGamePhaseChangedEvent);
        button.PressEvent += OnPressEvent;
        
    }

    private void OnDestroy()
    {
        button.PressEvent -= OnPressEvent;
    }

    private void OnPressEvent()
    {
        GameMode gamemode = gameModeService.CurrentGameMode;
        if (gamemode != null)
        {
            if (gamemode.MatchPhase == MatchPhase.PreGame)
                gamemode.AttemptToStartActiveGame();
            else if (gamemode.MatchPhase == MatchPhase.PostGame)
                gamemode.AttemptToReplayGame();
        }
    }

    private void OnGamePhaseChangedEvent(GameModePhaseChangedEvent obj)
    {
        switch (obj.Phase)
        {
            case MatchPhase.PreGame:
                gameObject.SetActive(true);
                break;
            case MatchPhase.Active:
                gameObject.SetActive(false);
                break;
            case MatchPhase.PostGame:
                gameObject.SetActive(true);
                break;
        }
    }
}