using System;
using UnityEngine;
using Utils.Core.Events;

public class PlayerPawnVisualsController : MonoBehaviour
{
    public static Color TeamNoneColor = Color.gray;
    public static Color Team1Color = Color.blue;
    public static Color Team2Color = Color.red;
    public static Color FFAColor = Color.yellow; // TODO: randomize and sync 
    public static float DeadAlpha = 0.4f;

    [SerializeField] private MeshRenderer[] renderers;

    private IPlayer player;
    private EventDispatcher localEventDispatcher;
    private Color currentColor;

    public void InjectDependencies(IPlayer player, EventDispatcher localEventDispatcher)
    {
        this.player = player;
        this.localEventDispatcher = localEventDispatcher;

        UpdateTeamColor(player.Team);
    }

    private void Start()
    {
        player.TeamChangeEvent += OnTeamChangeEvent;
        localEventDispatcher.Subscribe<PlayerDeathEvent>(OnDeathEvent);
        localEventDispatcher.Subscribe<PlayerRespawnEvent>(OnRespawnEvent);
    }

    private void OnDestroy()
    {
        player.TeamChangeEvent -= OnTeamChangeEvent;
        localEventDispatcher.Unsubscribe<PlayerDeathEvent>(OnDeathEvent);
        localEventDispatcher.Unsubscribe<PlayerRespawnEvent>(OnRespawnEvent);
    }

    private void OnTeamChangeEvent(Team oldTeam, Team newTeam)
    {
        UpdateTeamColor(newTeam);
    }

    public void UpdateTeamColor(Team newTeam)
    {
        switch (newTeam)
        {
            case Team.None:
                UpdateMeshColors(TeamNoneColor);
                break;
            case Team.FreeForAll:
                UpdateMeshColors(FFAColor);
                break;
            case Team.Team1:
                UpdateMeshColors(Team1Color);
                break;
            case Team.Team2:
                UpdateMeshColors(Team2Color);
                break;
            default:
                throw new NotImplementedException("No corresponding color for " + newTeam);
        }
    }

    public void UpdateMeshColors(Color color)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = color;
        }
        currentColor = color;
    }
    
    private void OnDeathEvent(PlayerDeathEvent @event)
    {
        UpdateAlpha(DeadAlpha);
    }


    private void OnRespawnEvent(PlayerRespawnEvent @event)
    {
        UpdateAlpha(1);
    }

    public void UpdateAlpha(float alpha)
    {
        Color color = currentColor;
        color.a = alpha;
        UpdateMeshColors(color);
    }
}
