using Photon.Pun;
using System.Collections;
using UnityEngine;
using Utils.Core.Services;

public class OrderBell : MonoBehaviour
{
    [SerializeField] private PhysicsButton button = null;
    [SerializeField] private DeliveryPoint deliveryPoint = null;
    [SerializeField] private float delayBetweenEndAndReplay = 2;

    private GameModeService gameModeService;

    private void Awake()
    {
        gameModeService = GlobalServiceLocator.Instance.Get<GameModeService>();
    }

    private void OnEnable()
    {
        button.PressEvent += OnPressEvent;
    }

    private void OnDisable()
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
            else if (gamemode.MatchPhase == MatchPhase.PostGame && ReplayDelayIsDone())
                gamemode.Replay();
            else if (gamemode.MatchPhase == MatchPhase.Active)
                deliveryPoint.DeliverDishesInTrigger();
        }
        else
        {
            deliveryPoint.DeliverDishesInTrigger();
        }
    }

    private bool ReplayDelayIsDone()
    {
        return (float)PhotonNetwork.Time - gameModeService.CurrentGameMode.MatchEndTimeStamp > delayBetweenEndAndReplay;
    }
}
