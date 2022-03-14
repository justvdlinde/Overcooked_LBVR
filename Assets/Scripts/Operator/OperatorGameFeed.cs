using System;
using UnityEngine;
using Utils.Core.Events;

public class OperatorGameFeed : MonoBehaviour
{
    private GlobalEventDispatcher globalEventDispatcher;

    public void InjectDependencies(GlobalEventDispatcher globalEventDispatcher)
    {
        this.globalEventDispatcher = globalEventDispatcher;
    }

    private void OnEnable()
    {
        globalEventDispatcher.Subscribe<PlayerDeathEvent>(OnPlayerDeathEvent);
        globalEventDispatcher.Subscribe<PlayerRespawnEvent>(OnPlayerRespawnEvent);
        globalEventDispatcher.Subscribe<GameModeChangedEvent>(OnGameModeChangedEvent);
        globalEventDispatcher.Subscribe<GameModePhaseChangedEvent>(OnGameModePhaseChangedEvent);
        globalEventDispatcher.Subscribe<GameOverEvent>(OnGameOverEvent);
        globalEventDispatcher.Subscribe<PlayerChangedTeamEvent>(OnPlayerChangedTeamEvent);
    }

    private void OnDisable()
    {
        globalEventDispatcher.Unsubscribe<PlayerDeathEvent>(OnPlayerDeathEvent);
        globalEventDispatcher.Unsubscribe<PlayerRespawnEvent>(OnPlayerRespawnEvent);
        globalEventDispatcher.Unsubscribe<GameModeChangedEvent>(OnGameModeChangedEvent);
        globalEventDispatcher.Unsubscribe<GameModePhaseChangedEvent>(OnGameModePhaseChangedEvent);
        globalEventDispatcher.Unsubscribe<GameOverEvent>(OnGameOverEvent);
        globalEventDispatcher.Unsubscribe<PlayerChangedTeamEvent>(OnPlayerChangedTeamEvent);
    }

    private void OnPlayerRespawnEvent(PlayerRespawnEvent @event)
    {
        Debug.Log(@event.Player.Name + " Respawned");
    }

    private void OnGameModePhaseChangedEvent(GameModePhaseChangedEvent @event)
    {
        switch (@event.Phase)
        {
            case MatchPhase.Countdown:
                Debug.Log("Startin game in 3 seconds");
                break;
            case MatchPhase.Active:
                Debug.Log("Play!");
                break;
            case MatchPhase.PostGame:
                Debug.Log("Game Over!");
                break;
        }
    }

    private void OnGameModeChangedEvent(GameModeChangedEvent @event)
    {
        Debug.Log("Game changed to " + @event.GameMode.Name);
    }

    private void OnGameOverEvent(GameOverEvent @event)
    {
        Debug.Log("Game Over because: " + @event.GameResult.EndType);
        if (@event.GameResult is TDMGameResult)
        {
            TDMGameResult result = @event.GameResult as TDMGameResult;
            if(result.IsDraw)
                Debug.LogFormat("Game is a draw");
            else
                Debug.LogFormat("Winner: {0} " + result.Winner);
        }
    }

    private void OnPlayerChangedTeamEvent(PlayerChangedTeamEvent @event)
    {
        Debug.LogFormat("{0} changed to {1}", @event.Player, @event.Team);
    }

    private void OnPlayerDeathEvent(PlayerDeathEvent @event)
    {
        PlayerDeathInfo info = @event.Info;
        IPlayer player = @event.Info.DeadPlayer;

        if(info is PlayerKilledInfo)
        {
            if (info.Damage is BulletDamage)
            {
                bool isHeadshot = (info.Damage as BulletDamage).IsHeadshot();
                Debug.LogFormat("{0} killed {1} " + ((isHeadshot) ? "by headshot" : ""), player.Name);
            }
            else
            {
                Debug.LogFormat("{0} killed {1}", player.Name);
            }
        }

        switch (info.Damage.DamageType)
        {
            case DamageType.EnemyPlayer:
                Debug.LogFormat("{0} killed {1}", player.Name);
                break;
            case DamageType.Unknown:
                Debug.LogFormat("{0} died from mysterious reasons", player.Name);
                break;
            case DamageType.FallDamage:
                Debug.LogFormat("{0} fell from too high", player.Name);
                break;
            case DamageType.ClippingPrevention:
                Debug.LogFormat("{0} tried to walk through the environment", player.Name);
                break;
            default:
                Debug.LogFormat("No corresponding feed for type of {0}", info.Damage.DamageType);
                break;
        }
    }
}
