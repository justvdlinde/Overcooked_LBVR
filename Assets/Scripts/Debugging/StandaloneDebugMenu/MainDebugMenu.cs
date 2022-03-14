using UnityEngine;
using Utils.Core.FPSCounter;
using Utils.Core.Injection;
using Utils.Core.Services;

public class MainDebugMenu : MonoBehaviour
{
    [SerializeField] private KeyCode ToggleKey = KeyCode.F10;
    [SerializeField] private bool show = false;
    [SerializeField] private bool drawDeveloperOptions = false;

    private IDebugMenu currentOpenMenu;
    private PhotonNetworkDebugMenu networkDebugger;
    private PlayersDebugMenu playersDebugger;
    private GameModeDebugMenu gamemodeDebugger;
    private XRInputDebugMenu inputDebugger;
    private SceneDebugMenu sceneDebugger;
    private ProfilerDebugMenu profilerDebugger;
    private INetworkService networkService;

    [RuntimeInitializeOnLoadMethod]
    private static void OnRuntimeMethodLoad()
    {
        GameObject g = new GameObject("Game Debug Menu");
        g.AddComponent<MainDebugMenu>();
        DontDestroyOnLoad(g);
    }

    private void Awake()
    {
        DependencyInjector injector = new DependencyInjector("DebugMenu");
        networkDebugger = injector.CreateType<PhotonNetworkDebugMenu>();
        playersDebugger = injector.CreateType<PlayersDebugMenu>();
        gamemodeDebugger = injector.CreateType<GameModeDebugMenu>();
        inputDebugger = injector.CreateType<XRInputDebugMenu>();
        sceneDebugger = injector.CreateType<SceneDebugMenu>();
        profilerDebugger = injector.CreateType<ProfilerDebugMenu>();

        networkService = GlobalServiceLocator.Instance.Get<INetworkService>();
        drawDeveloperOptions = Debug.isDebugBuild;

        if (FPSCounter.CounterInstance == null)
            FPSCounter.CreateCounterInstance();
    }

    private void Update()
    {
        if (Input.GetKeyDown(ToggleKey))
            show = !show;
    }

    private void OnGUI()
    {
        if (!show)
            return;

        if (currentOpenMenu == null)
        {
            GUILayout.BeginVertical("Debug Menu", "window", GUILayout.Width(300));

            DrawGeneralInfo();
            GUILayout.Space(10);

            DrawMenuButton(profilerDebugger, "Profiler");
            DrawMenuButton(networkDebugger, "Network");
            DrawMenuButton(playersDebugger, "Players");
            DrawMenuButton(gamemodeDebugger, "GameMode");

            if (drawDeveloperOptions)
                DrawMenuButton(inputDebugger, "XR Input");

            DrawMenuButton(sceneDebugger, "Scenes");

            DrawLogButton();

            GUILayout.Space(10);
            if (GUILayout.Button("Close (F10)"))
                show = !show;
            GUILayout.EndVertical();
        }
        else
        {
            if (GUILayout.Button("Back", GUILayout.Width(80)))
            {
                currentOpenMenu = null;
                return;
            }
            currentOpenMenu.OnGUI(drawDeveloperOptions);
        }
    }

    private void DrawGeneralInfo()
    {
        GUILayout.BeginVertical();
        GUILayout.Label(string.Format("FPS: {0:0}", FPSCounter.CurrentFPS));
        GUILayout.Label("Game Version: " + Application.version); 
        GUILayout.EndVertical();
    }

    private void DrawMenuButton(IDebugMenu menu, string buttonLabel)
    {
        if (GUILayout.Button(buttonLabel))
        {
            if (currentOpenMenu != null)
                currentOpenMenu.Close();

            currentOpenMenu = menu;
            currentOpenMenu.Open();
        }
    }

    private void DrawLogButton()
    {
        if(GUILayout.Button("Open Logs"))
        {
            LogHelper.OpenLogs();
        }
    }
}
