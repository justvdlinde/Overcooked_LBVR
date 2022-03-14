using UnityEngine;
using Utils.Core.FPSCounter;

public class ProfilerDebugMenu : IDebugMenu
{
    public void Open()
    {
        if (FPSCounter.CounterInstance == null)
            FPSCounter.CreateCounterInstance();

        if (ProfilerHelper.Instance == null)
            ProfilerHelper.CreateInstance();
    }

    public void Close() { }

    public void OnGUI(bool drawDeveloperOptions)
    {
        GUILayout.BeginVertical("box", GUILayout.MinWidth(250));
        GUILayout.Label(string.Format("FPS: {0:0}", FPSCounter.CurrentFPS));
        GUILayout.Label("Draw Calls: " + ProfilerHelper.DrawCalls);
        GUILayout.Label("Batches: " + ProfilerHelper.BatchesCount);
        GUILayout.Label("Tris: " + ProfilerHelper.TrisCount);
        GUILayout.Label("Vertices: " + ProfilerHelper.VerticesCount);
        GUILayout.Label("Frame Time: " + ProfilerHelper.FrameTime);
        GUILayout.Label("GC Time: " + ProfilerHelper.GarbageCollectionTime);
        GUILayout.Label("System Memory: " + ProfilerHelper.SystemMemory);
        GUILayout.EndVertical();
    }
}
