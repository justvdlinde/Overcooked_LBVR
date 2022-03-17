using System;
using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Events;
using Utils.Core.Services;

/// <summary>
/// Service for managing all connected players, operator and spectators
/// </summary>
public class PlayersManager : IService
{
    public IClient LocalPlayer { get; private set; }
    public List<IPlayer> AllPlayers { get; private set; }
    public int PlayerCount => AllPlayers.Count;
    public Action<PlayerJoinEvent> PlayerJoinEvent;
    public Action<PlayerLeftEvent> PlayerLeftEvent;
    public List<Operator> Operators { get; private set; }
    public List<Spectator> Spectators { get; private set; }

    private Dictionary<string, IPlayer> PlayerIdTable;
    private readonly GlobalEventDispatcher globalEventDispatcher;

    public PlayersManager(GlobalEventDispatcher globalEventDispatcher)
    {
        AllPlayers = new List<IPlayer>();
        PlayerIdTable = new Dictionary<string, IPlayer>();
        Operators = new List<Operator>();
        Spectators = new List<Spectator>();

        this.globalEventDispatcher = globalEventDispatcher;
        this.globalEventDispatcher.Subscribe<ClientJoinEvent>(OnClientJoinedEvent);
        this.globalEventDispatcher.Subscribe<ClientLeftEvent>(OnClientLeftEvent);
    }

    public void Dispose()
    {
        globalEventDispatcher.Unsubscribe<ClientJoinEvent>(OnClientJoinedEvent);
        globalEventDispatcher.Unsubscribe<ClientLeftEvent>(OnClientLeftEvent);
    }

    private void OnClientLeftEvent(ClientLeftEvent @event)
    {
        Debug.Log("Client left server, client is " + @event.Data.Client.GetType());
        IClient client = @event.Data.Client;
        if (client is IPlayer)
            RemovePlayer(client as IPlayer);
        else if (client is Operator)
            RemoveOperator(client as Operator);
        else if (client is Spectator)
            RemoveSpectator(client as Spectator);
        else
            throw new NotImplementedException("No support for client type of " + client);
    }

    private void OnClientJoinedEvent(ClientJoinEvent @event)
    {
        IClient client = @event.Data.Client;
        if (client is IPlayer)
            AddPlayer(client as IPlayer);
        else if (client is Operator)
            AddOperator(client as Operator);
        else if (client is Spectator)
            AddSpectator(client as Spectator);
        else
            throw new NotImplementedException("No support for client type of " + client);
    }

    public void AddPlayer(IPlayer player)
    {
        if(player == null)
            throw new NullReferenceException("Player is null");

        AllPlayers.Add(player);
        PlayerIdTable.Add(player.ID, player);

        if (player.IsLocal)
            LocalPlayer = player as IClient;

        PlayerJoinEvent @event = new PlayerJoinEvent(player);
        PlayerJoinEvent?.Invoke(@event);
        globalEventDispatcher.Invoke(@event);
    }

    public void RemovePlayer(IPlayer player)
    {
        AllPlayers.Remove(player);
        PlayerIdTable.Remove(player.ID);

        if (player == LocalPlayer)
            LocalPlayer = null;

        PlayerLeftEvent @event = new PlayerLeftEvent(player);
        PlayerLeftEvent?.Invoke(@event);
        globalEventDispatcher.Invoke(@event);
        player.Dispose();
    }

    public IPlayer GetPlayerById(string id)
    {
        return PlayerIdTable[id];
    }

    public IPlayer CreateDummyPlayer(bool instantiatePawn)
    {
        DummyFactory factory = new DummyFactory(instantiatePawn);
        DummyPlayer player = factory.Construct();
        AddPlayer(player);
        return player;
    }

    public IPlayer CreateDummyPlayer(string id, bool instantiatePawn)
    {
        DummyFactory factory = new DummyFactory(instantiatePawn, id);
        DummyPlayer player = factory.Construct();
        AddPlayer(player);
        return player;
    }

    public void RemoveAllDummyPlayers()
    {
        for (int i = 0; i < AllPlayers.Count; i++)
        {
            if (AllPlayers[i] is DummyPlayer)
                RemovePlayer(AllPlayers[i]);
        }
    }

    public void AddOperator(Operator @operator)
    {
        Operators.Add(@operator);

        if (@operator.IsLocal)
            LocalPlayer = @operator;

        globalEventDispatcher.Invoke(new OperatorJoinEvent(@operator));
    }

    public void RemoveOperator(Operator @operator)
    {
        Operators.Remove(@operator);

        if (@operator.IsLocal)
            LocalPlayer = null;

        globalEventDispatcher.Invoke(new OperatorLeftEvent(@operator));
    }

    public void AddSpectator(Spectator spectator)
    {
        Spectators.Add(spectator);

        if (spectator.IsLocal)
            LocalPlayer = spectator;

        globalEventDispatcher.Invoke(new SpectatorJoinEvent(spectator));
    }

    public void RemoveSpectator(Spectator spectator)
    {
        Spectators.Remove(spectator);
        if (spectator.IsLocal)
            LocalPlayer = null;
        globalEventDispatcher.Invoke(new SpectatorLeftEvent(spectator));
    }
}
