using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using Utils.Core.FPSCounter;

/************************************************************************************************************
* Source: https://docs.unity3d.com/2020.2/Documentation/ScriptReference/Unity.Profiling.ProfilerRecorder.html
*************************************************************************************************************/

/// <summary>
/// Get profile related information, make sure to call <see cref="CreateInstance"/> before using
/// </summary>
public class ProfilerHelper : MonoBehaviour
{
    private struct StatInfo
    {
        public ProfilerCategory category;
        public string name;
        public ProfilerMarkerDataUnit unit;
    }

    public static ProfilerHelper Instance { get; protected set; }
    public static string FrameTime { get; private set; }
    public static string GarbageCollectionTime { get; private set; }
    public static string SystemMemory { get; private set; }
    public static string DrawCalls { get; private set; }
    public static string BatchesCount { get; private set; }
    public static string TrisCount { get; private set; }
    public static string VerticesCount { get; private set; }

    private ProfilerRecorder systemMemoryRecorder;
    private ProfilerRecorder gcMemoryRecorder;
    private ProfilerRecorder mainThreadTimeRecorder;
    private ProfilerRecorder drawCallsCountRecorder;
    private ProfilerRecorder batchesCountRecorder;
    private ProfilerRecorder trisRecorder;
    private ProfilerRecorder verticesRecorder;

    /// <summary>
    /// Creates a gameobject required for calculating fps
    /// </summary>
    public static ProfilerHelper CreateInstance()
    {
        ProfilerHelper instance = new GameObject("ProfilerHelper", typeof(ProfilerHelper)).GetComponent<ProfilerHelper>();
        DontDestroyOnLoad(instance);
        return instance;
    }

    protected virtual void Awake()
    {
        if (Instance != null)
            Destroy(Instance);
        Instance = this;
    }

    private void Start()
    {
        systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
        gcMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
        mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
        drawCallsCountRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
        batchesCountRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
        trisRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
        verticesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");
    }

    private void OnDestroy()
    {
        systemMemoryRecorder.Dispose();
        gcMemoryRecorder.Dispose();
        mainThreadTimeRecorder.Dispose();
        drawCallsCountRecorder.Dispose();
        batchesCountRecorder.Dispose();
        trisRecorder.Dispose();
        verticesRecorder.Dispose();
    }

    private void Update()
    {
        FrameTime = ($"{GetRecorderFrameAverage(mainThreadTimeRecorder) * (1e-6f):F1} ms");
        GarbageCollectionTime = ($"{gcMemoryRecorder.LastValue / (1024 * 1024)} MB");
        SystemMemory = ($"{systemMemoryRecorder.LastValue / (1024 * 1024)} MB");
        DrawCalls = ($"{drawCallsCountRecorder.LastValue}");
        BatchesCount = ($"{batchesCountRecorder.LastValue}");
        TrisCount = ($"{trisRecorder.LastValue}");
        VerticesCount = ($"{verticesRecorder.LastValue}");
    }

    private static double GetRecorderFrameAverage(ProfilerRecorder recorder)
    {
        int samplesCount = recorder.Capacity;
        if (samplesCount == 0)
            return 0;

        double r = 0;
        List<ProfilerRecorderSample> samples = new List<ProfilerRecorderSample>(samplesCount);
        recorder.CopyTo(samples);
        for (int i = 0; i < samples.Count; ++i)
            r += samples[i].Value;
        r /= samplesCount;

        return r;
    }

    public static void PrintProfilerStats()
    {
        var availableStatHandles = new List<ProfilerRecorderHandle>();
        ProfilerRecorderHandle.GetAvailable(availableStatHandles);

        var availableStats = new List<StatInfo>(availableStatHandles.Count);
        foreach (var h in availableStatHandles)
        {
            var statDesc = ProfilerRecorderHandle.GetDescription(h);
            var statInfo = new StatInfo()
            {
                category = statDesc.Category,
                name = statDesc.Name,
                unit = statDesc.UnitType
            };
            availableStats.Add(statInfo);
        }
        availableStats.Sort((a, b) =>
        {
            var result = string.Compare(a.category.ToString(), b.category.ToString());
            if (result != 0)
                return result;

            return string.Compare(a.name, b.name);
        });

        var sb = new StringBuilder("Available stats:\n");
        foreach (var s in availableStats)
        {
            sb.AppendLine($"{s.category.ToString()}\t\t - {s.name}\t\t - {s.unit}");
        }

        //FileManager.WriteToFile("AvailableStats.txt", sb.ToString());
        Debug.Log(sb.ToString());
    }
}