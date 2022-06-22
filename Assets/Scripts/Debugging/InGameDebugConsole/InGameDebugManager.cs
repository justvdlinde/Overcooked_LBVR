using Unity.Profiling;
using UnityEngine;
using Utils.Core.Attributes;
using Utils.Core.FPSCounter;
using Utils.Core.Services;

public class InGameDebugManager : MonoBehaviour
{
    [SerializeField] private GameObject debugConsolePrefab = null;
    [SerializeField] private Transform debugConsoleRoot = null;

    [SerializeField] private GameObject guiPanel = null;
    [SerializeField] private Transform guiPanelRoot = null;

    [SerializeField] private bool show = true;

    private GameObject debugConsoleInstance;
    private GameObject guiPanelInstance;

    private void Awake()
    {
        if(Debug.isDebugBuild)
        {
            if(debugConsolePrefab != null)
                debugConsoleInstance = Instantiate(debugConsolePrefab, debugConsoleRoot);

            if(guiPanel != null)
                guiPanelInstance = Instantiate(guiPanel, guiPanelRoot);

            if (FPSCounter.CounterInstance == null)
                FPSCounter.CreateCounterInstance();

            if (ProfilerHelper.Instance == null)
                ProfilerHelper.CreateInstance();
        }

        ShowObjects(show);
    }

    private void Start()
    {
        //GUIWorldSpace.AddButton("Print Test", () => Debug.Log("Test"));

        // TO DO: remove
        PhotonNetworkService n = GlobalServiceLocator.Instance.Get<INetworkService>() as PhotonNetworkService;
        GUIWorldSpace.AddButton("Disconnect", () => n.Disconnect());
    }

    [Button]
    private void Toggle()
    {
        show = !show;
        ShowObjects(show);
    }

    private void ShowObjects(bool show)
    {
        if(debugConsoleInstance != null)
            debugConsoleInstance.SetActive(show);
        if(guiPanelInstance != null)
            guiPanelInstance.SetActive(show);
    }

    private void Update()
    {
        GUIWorldSpace.Log("FPS: " + FPSCounter.CurrentFPS.ToString());
        GUIWorldSpace.Log("Time: " + Time.time);
        GUIWorldSpace.Log("WorldPos: " + transform.position);
        GUIWorldSpace.Log("DrawCalls: " + ProfilerHelper.DrawCalls);
        GUIWorldSpace.Log("Batches: " + ProfilerHelper.BatchesCount);
        GUIWorldSpace.Log("Tris: " + ProfilerHelper.TrisCount);
        GUIWorldSpace.Log("Vertices " + ProfilerHelper.VerticesCount);

        if (XRInput.GetSecondaryButtonUp(Hand.Left))
            Toggle();
    }
}
