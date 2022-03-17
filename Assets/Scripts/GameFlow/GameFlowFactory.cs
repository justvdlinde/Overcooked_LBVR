using UnityEngine;
using Utils.Core.Injection;

[System.Serializable]
public class GameFlowFactory : IFactory<GameFlow>
{
    [SerializeField] protected OperatorFlow operatorFlow = null; 
    [SerializeField] protected PlayerFlow playerFlow = null;

    public DependencyInjector injector;

    public GameFlow Construct()
    {
        GameFlow flow;

        switch (GamePlatform.GameType)
        {
            case ClientType.Player:
                flow = playerFlow;
                break;
            case ClientType.Operator:
                flow = operatorFlow;
                break;
            case ClientType.Spectator:
                throw new System.NotImplementedException();
            default:
                throw new System.NotImplementedException(GamePlatform.GameType + " is not supported");
        }

        GameFlow flowInstance = injector.InstantiateGameObject(flow);
        return flowInstance;
    }
}

