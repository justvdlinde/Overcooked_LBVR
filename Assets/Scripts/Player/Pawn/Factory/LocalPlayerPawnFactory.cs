using UnityEngine;
using Utils.Core.Injection;

public class LocalPlayerPawnFactory : IFactory<PlayerPawn>
{
    private readonly DependencyInjector injector;

    public LocalPlayerPawnFactory(DependencyInjector injector)
    {
        this.injector = injector;
    }

    public PlayerPawn Construct()
    {
        // Here we can create different prefabs for different platforms, such as Spree

        PlayerPawn pawn = injector.InstantiateGameObject(Resources.Load<PlayerPawn>("LocalPlayerPawnPhysics"));
        pawn.name = "Local Player Pawn";
        return pawn;
    }
}
