using Photon.Pun;
using UnityEngine;
using Utils.Core.Events;

public class StoryGameMode : GameMode
{
    public override string Name => "Story";
    public override GameModeEnum GameModeEnum => GameModeEnum.Story;

    public StoryGameMode(GlobalEventDispatcher globalEventDispatcher, INetworkService networkService) : base(globalEventDispatcher, networkService) { }

    public override void Setup()
    {
        Scoreboard = new StoryGameScores();
        base.Setup();
    }

    public override IGameResult GetGameResultData()
    {
        return new StoryGameResult(this, Scoreboard as StoryGameScores);
    }

    private void OnLastOrderServed()
    {
        Debug.LogFormat("Last dish served, Game Over");
        if (PhotonNetwork.IsMasterClient)
            EndGame();
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}
