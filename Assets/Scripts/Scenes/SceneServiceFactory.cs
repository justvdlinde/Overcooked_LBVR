using Utils.Core.Injection;
using Utils.Core.SceneManagement;
using Utils.Core.Services;

public class SceneServiceFactory : IServiceFactory<SceneService>
{
    private readonly DependencyInjector injector;

    public SceneServiceFactory(DependencyInjector injector)
    {
        this.injector = injector;
    }

    public SceneService Construct()
    {
        return injector.CreateType<NetworkedSceneService>();
    }
}
