using Unity.Profiling;
using UnityEngine;
using Utils.Core.FPSCounter;

public class InGameDebugManager : MonoBehaviour
{
    [SerializeField] private GameObject debugConsolePrefab = null;
    [SerializeField] private Transform debugConsoleRoot = null;

    [SerializeField] private GameObject guiPanel = null;
    [SerializeField] private Transform guiPanelRoot = null;

    private void Awake()
    {
        if(Debug.isDebugBuild)
        {
            if(debugConsolePrefab != null)
                Instantiate(debugConsolePrefab, debugConsoleRoot);

            if(guiPanel != null)
                Instantiate(guiPanel, guiPanelRoot);

            if (FPSCounter.CounterInstance == null)
                FPSCounter.CreateCounterInstance();

            if (ProfilerHelper.Instance == null)
                ProfilerHelper.CreateInstance();
        }

    }

    private void Start()
    {
        GUIWorldSpace.AddButton("Print Test", () => Debug.Log("Test"));
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
    }
}
