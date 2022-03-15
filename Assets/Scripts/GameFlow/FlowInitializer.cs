using UnityEngine;
using Utils.Core.Injection;

public class FlowInitializer : MonoBehaviour
{
    [SerializeField] private GameFlowFactory flowFactory = null;

    private void Awake()
    {
        DependencyInjector injector = new DependencyInjector("GameFlow");
        flowFactory.injector = injector;
        flowFactory.Construct();
    }
}
