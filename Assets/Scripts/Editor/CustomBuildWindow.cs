using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.XR.Management;
using Debug = UnityEngine.Debug;

public class CustomBuildWindow : EditorWindow
{
    private static CustomBuildWindow window;
    private readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
    private const string validIcon = "Valid";
    private const string invalidIcon = "Invalid";
    private const string levelsPath = "Assets/Data/Levels/Resources/GameLevels.Asset";
    private const string networkConfigPath = "Assets/Data/Network/NetworkConfig.Asset";

    private Vector2 scrollPosition;
    private GUIStyle headerStyle;
    private NetworkConfig networkConfig;

    [MenuItem("Build/Build Checklist")]
    static void Open()
    {
        GetWindow(Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
        window = GetWindow<CustomBuildWindow>("Build Checklist");
        window.Show(true);
    }

    private void OnEnable()
    {
        if (!PlatformPicker.IsInitialized)
            PlatformPicker.Init();
    }

    private void OnGUI()
    {
        if (window == null)
            window = this;

        if (headerStyle == null)
        {
            headerStyle = EditorStyles.boldLabel;
            headerStyle.fontSize = 12;
        }

        if (networkConfig == null)
            networkConfig = AssetDatabase.LoadAssetAtPath(networkConfigPath, typeof(NetworkConfig)) as NetworkConfig;

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Build Name", Application.productName);
        if (GUILayout.Button("Change", GUILayout.MaxWidth(60)))
            SettingsService.OpenProjectSettings("Project/Player");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Version", Application.version);
        if (GUILayout.Button("Change", GUILayout.MaxWidth(60)))
            SettingsService.OpenProjectSettings("Project/Player");
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        DrawScenesPanel();

        DrawPlatformInfo();

        GUILayout.Space(10);
        DrawChecklist();

        GUILayout.Space(10);
        DrawDirectivesPanel();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Build", GUILayout.MaxWidth(window.position.width / 2))) 
            BuildGame();
        if (GUILayout.Button("Build And Run", GUILayout.MaxWidth(window.position.width / 2)))
            BuildGame(true);
        GUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
    }

    private void DrawScenesPanel()
    {
        GUILayout.Label("Scenes", headerStyle);
        GUILayout.BeginVertical("HelpBox");
        string[] scenes = GetBuildScenes();
        for(int i = 0; i < scenes.Length; i++)
        {
            string scene = scenes[i];
            if (scene.StartsWith("Assets/"))
                scene = scene.Remove(0, 7);

            GUILayout.BeginHorizontal();
            GUILayout.Label(i + ": " + scene);
            GUILayout.FlexibleSpace();
            GUILayout.Label(EditorGUIUtility.IconContent((i % 2 == 0) ? validIcon : invalidIcon));
            if (GUILayout.Button("Select"))
                Selection.activeObject = AssetDatabase.LoadAssetAtPath(scene, typeof(UnityEngine.Object));
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Select Levels", GUILayout.MaxWidth(100)))
            Selection.activeObject = AssetDatabase.LoadAssetAtPath(levelsPath, typeof(UnityEngine.Object));
        GUILayout.EndHorizontal();
    }

    private void DrawDirectivesPanel()
    {
        GUILayout.Label("Platform Directives", headerStyle);
        GUILayout.BeginVertical();

        string directives = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        string[] directivesClean = directives.Split(';');
        GUI.enabled = false;

        GUILayout.BeginVertical("HelpBox");
        foreach (string define in directivesClean)
        {
            GUILayout.Label(define);
        }
        GUILayout.EndVertical();
        GUI.enabled = true;

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Change", GUILayout.MaxWidth(100)))
            Utils.Core.ScriptingDefineSymbols.ScriptingDefineSymbolsEditorWindow.OpenWindow();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    private void DrawPlatformInfo()
    {
        GUILayout.Label("Platform", headerStyle);
        GUI.enabled = false;
        EditorGUILayout.LabelField("Platform", PlatformPicker.BuildPlatform.ToString());
        if (PlatformPicker.BuildPlatform == BuildPlatform.Android)
            EditorGUILayout.LabelField("Device", PlatformPicker.AndroidDevice.ToString());
        EditorGUILayout.LabelField("GameType", PlatformPicker.GameType.ToString());
        EditorGUILayout.LabelField("Client Platform", PlatformPicker.ClientPlatforms[PlatformPicker.SelectedClientPlatform].ToString());
        GUI.enabled = true;

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Change", GUILayout.MaxWidth(100)))
            PlatformPicker.Open();
        GUILayout.EndHorizontal();
    }

    private void DrawChecklist()
    {
        GUILayout.Label("Options", headerStyle);
        GUILayout.BeginHorizontal();
        if (XRGeneralSettings.Instance != null)
            XRGeneralSettings.Instance.InitManagerOnStart = EditorGUILayout.Toggle("Initialze XR On Startup", XRGeneralSettings.Instance.InitManagerOnStart);
        else
            EditorGUILayout.LabelField("Initialze XR On Startup", "Error trying to load");
        if (GUILayout.Button("Select", GUILayout.MaxWidth(60)))
            SettingsService.OpenProjectSettings("Project/XR Plug-in Management");
        GUILayout.EndHorizontal();

        GUI.enabled = false;
        EditorGUILayout.Toggle("Licensing", false);
        EditorGUILayout.Toggle("Show Splash", false);
        GUI.enabled = true;

        GUILayout.BeginHorizontal();
        if (networkConfig != null)
        {
            networkConfig.connectionType = (ConnectionType)EditorGUILayout.EnumPopup("Networking", networkConfig.connectionType);
            if (GUILayout.Button("Select", GUILayout.MaxWidth(60)))
                Selection.activeObject = networkConfig;
        }
        else
        {
            GUILayout.Label("No NetworkConfig found");
        }
        GUILayout.EndHorizontal();
    }

    private void BuildGame(bool autoRun = false)
    {
        BuildPlayerOptions buildPlayerOptions = GetBuildPlayerOptions();
        string[] scenes = GetBuildScenes();
        if (scenes.Length == 0)
        {
            Debug.LogWarning("No scenes in build settings!");
            return;
        }

        string path = EditorUtility.SaveFolderPanel("Choose Location", "", "");
        if (path == string.Empty)
            return;

        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.target = EditorUserBuildSettings.activeBuildTarget;
        buildPlayerOptions.locationPathName = path + "/" + Application.productName + GetBuildFileType(buildPlayerOptions.target);

        if (!buildPlayerOptions.options.HasFlag(BuildOptions.ShowBuiltPlayer))
            buildPlayerOptions.options = AddOption(buildPlayerOptions.options, BuildOptions.ShowBuiltPlayer);

        if(autoRun && !buildPlayerOptions.options.HasFlag(BuildOptions.AutoRunPlayer))
            buildPlayerOptions.options = AddOption(buildPlayerOptions.options, BuildOptions.AutoRunPlayer);

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.LogFormat("Build succeeded \nFinal size: {0} \nLocation: {1}", GetSizeSuffix((long)summary.totalSize), buildPlayerOptions.locationPathName);
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("<color=red>Build failed!</color>");
        }
    }

    public BuildOptions AddOption(BuildOptions options, BuildOptions add)
    {
        return options |= add;
    }

    public BuildOptions RemoveOption(BuildOptions options, BuildOptions remove)
    {
        return options &= ~remove;
    }

    private BuildPlayerOptions GetBuildPlayerOptions(bool askForLocation = false, BuildPlayerOptions defaultOptions = new BuildPlayerOptions())
    {
        // Get static internal "GetBuildPlayerOptionsInternal" method
        MethodInfo method = typeof(BuildPlayerWindow.DefaultBuildMethods).GetMethod("GetBuildPlayerOptionsInternal", BindingFlags.NonPublic | BindingFlags.Static);

        // invoke internal method
        return (BuildPlayerOptions)method.Invoke(null, new object[] { askForLocation, defaultOptions });
    }

    private string[] GetBuildScenes()
    {
        List<string> scenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
                scenes.Add(scene.path);
        }
        return scenes.ToArray();
    }
    
    private string GetBuildFileType(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return ".exe";
            case BuildTarget.Android:
                return ".apk";
            default:
                throw new NotImplementedException("No file type implemented for " + target.ToString());
        }
    }

    private string GetSizeSuffix(long value, int decimalPlaces = 1)
    {
        if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
        if (value < 0) { return "-" + GetSizeSuffix(-value, decimalPlaces); }
        if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

        // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
        int mag = (int)Math.Log(value, 1024);

        // 1L << (mag * 10) == 2 ^ (10 * mag) 
        // [i.e. the number of bytes in the unit corresponding to mag]
        decimal adjustedSize = (decimal)value / (1L << (mag * 10));

        // make adjustment when the value is large enough that
        // it would round up to 1000 or more
        if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
        {
            mag += 1;
            adjustedSize /= 1024;
        }

        return string.Format("{0:n" + decimalPlaces + "} {1}",
            adjustedSize,
            SizeSuffixes[mag]);
    }
}
