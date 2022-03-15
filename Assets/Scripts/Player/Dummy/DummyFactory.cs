using System;
using Utils.Core.Events;
using Utils.Core.Injection;

public class DummyFactory : IFactory<DummyPlayer>
{
    private readonly bool instantiatePawn;
    private readonly string id;

    public DummyFactory(bool instantiatePawn) 
    {
        this.instantiatePawn = instantiatePawn;
        id = GenerateGuid();
    }

    public DummyFactory(bool instantiatePawn, string id) 
    {
        this.instantiatePawn = instantiatePawn;
        this.id = id;
    }

    protected string GenerateGuid()
    {
        return Guid.NewGuid().ToString();
    }

    public DummyPlayer Construct()
    {
        DependencyInjector injector = new DependencyInjector("DummyPlayer");
        EventDispatcher localEventDispatcher = new EventDispatcher("DummyPlayer");
        injector.RegisterInstance<EventDispatcher>(localEventDispatcher);

        DummyPlayer player;
        if (instantiatePawn)
            player = new PhotonDummyPlayer(injector, id, instantiatePawn);
        else
            player = new DummyPlayer(injector, id, instantiatePawn);

        return player;
    }
}
