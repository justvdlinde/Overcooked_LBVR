using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Management;

public class PlatformPicker : EditorWindow
{
    private const string GameTypePrefsKey = "GameType";
    private const string picoManifest = @"AndroidManifestFiles\Pico\AndroidManifest.xml";
    private const string oculusManifest = @"AndroidManifestFiles\Oculus\AndroidManifest.xml";
    private const string currentAndroidManifest = @"Assets\Plugins\Android\AndroidManifest.xml";
    private const string androidManifestDirectory = @"Assets\Plugins\Android\AndroidManifest.xml";

    public static List<string> AllDirectives { get; private set; }
    private readonly string[] allPlatformDirectives = new string[]
    {
        ScriptingDefineSymbols.CLIENT,
        ScriptingDefineSymbols.OPERATOR,
        ScriptingDefineSymbols.SPECTATOR,
        ScriptingDefineSymbols.OCULUS,
        ScriptingDefineSymbols.PICO,
        ScriptingDefineSymbols.SPREE,
        ScriptingDefineSymbols.HEROZONE
    };

    public static int SelectedClientPlatform { get; private set; } = 0;
    public static readonly string[] ClientPlatforms = new string[]
    {
        "None",
        ScriptingDefineSymbols.SPREE,
        ScriptingDefineSymbols.HEROZONE,
    };

    public static bool IsInitialized { get; private set; }
    public static BuildPlatform BuildPlatform { get; private set; } = BuildPlatform.Android;
    public static AndroidDevice AndroidDevice { get; private set; } = AndroidDevice.Oculus;
    public static ClientType GameType { get; private set; } = ClientType.Player;
    private static BuildPlatform prevBuildPlatform;

    private Vector2 scrollPosition;
    private GUIStyle headerStyle;
    private bool valuesHaveChanged;

    [MenuItem("Platform/Platform Picker")]
    public static void Open()
    {
        EditorWindow window = GetWindow<PlatformPicker>("Platform Picker");
        window.Show(); 
    }

    private void OnEnable()
    {
        if(!IsInitialized)
            Init();
    }

    public static void Init()
    {
        if (!IsInitialized)
        {
            GameType = (ClientType)EditorPrefs.GetInt(GameTypePrefsKey, 0);
            UpdateScriptDefineSymbolsList(EditorUserBuildSettings.selectedBuildTargetGroup);

            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows)
                BuildPlatform = BuildPlatform.Windows;
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                BuildPlatform = BuildPlatform.Android;
            prevBuildPlatform = BuildPlatform;
            IsInitialized = true;

            for (int i = 0; i < ClientPlatforms.Length; i++)
            {
                if (AllDirectives.Contains(ClientPlatforms[i]))
                {
                    SelectedClientPlatform = i;
                    break;
                }
            } 
        }
    }

    private void OnGUI()
    {
        if(headerStyle == null)
        {
            headerStyle = EditorStyles.boldLabel;
            headerStyle.fontSize = 12;
            //contentStyle.alignment = TextAnchor.MiddleCenter;
        }

        EditorGUILayout.BeginVertical();
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        GUILayout.Label("Current Platform: " + EditorUserBuildSettings.selectedBuildTargetGroup.ToString());
        GUILayout.Space(10);
        GUILayout.Label("Platform Settings", headerStyle);

        EditorGUI.BeginChangeCheck();
        BuildPlatform = (BuildPlatform)EditorGUILayout.EnumPopup("Build Platform", BuildPlatform);
        if (BuildPlatform == BuildPlatform.Android)
            AndroidDevice = (AndroidDevice)EditorGUILayout.EnumPopup("Device", AndroidDevice);
        GameType = (ClientType)EditorGUILayout.EnumPopup("Game Type", GameType);
        SelectedClientPlatform = EditorGUILayout.Popup("Client Platform", SelectedClientPlatform, ClientPlatforms);
        if (EditorGUI.EndChangeCheck())
            valuesHaveChanged = true;

        GUILayout.Space(10);
        GUI.enabled = valuesHaveChanged;
        if (GUILayout.Button("Update Project Settings"))
            ApplySettings();
        GUI.enabled = true;
        GUILayout.Space(20);

        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Current Platform Directives", headerStyle);

        if (GUILayout.Button("Open Directive Editor"))
            Utils.Core.ScriptingDefineSymbols.ScriptingDefineSymbolsEditorWindow.OpenWindow();
        GUILayout.EndHorizontal();

        string directives = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        string[] directivesClean = directives.Split(';');
        GUI.enabled = false;

        GUILayout.BeginVertical("HelpBox");
        foreach (string define in directivesClean)
        {
            GUILayout.Label(define);
        }
        GUI.enabled = true;
        GUILayout.EndVertical();
        GUILayout.EndVertical();

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void ApplySettings()
    {
        EditorPrefs.SetInt(GameTypePrefsKey, (int)GameType);
        valuesHaveChanged = false;
        RemovePlatformSpecificDefineSymbols();

        switch (BuildPlatform)
        {
            case BuildPlatform.Android:
                UpdatePlatform(BuildTargetGroup.Android, BuildTarget.Android);
                break;
            case BuildPlatform.Windows:
                UpdatePlatform(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows);
                break;
            default:
                Debug.LogError(ClientPlatforms + " is not implemented");
                break;
        }

        if (SelectedClientPlatform != 0)
            AddDirective(ClientPlatforms[SelectedClientPlatform]);

        switch (GameType)
        {
            case ClientType.Player:
                AddDirective(ScriptingDefineSymbols.CLIENT);
                break;
            case ClientType.Operator:
                AddDirective(ScriptingDefineSymbols.OPERATOR);
                break;
            case ClientType.Spectator:
                AddDirective(ScriptingDefineSymbols.SPECTATOR);
                break;
            default:
                Debug.LogError(GameType + " is not implemented!");
                return;
        }
        UpdateScriptingDefineSymbolsToBuildTarget(EditorUserBuildSettings.selectedBuildTargetGroup);
    }

    private void UpdatePlatform(BuildTargetGroup buildTargetGroup, BuildTarget buildTarget)
    {
        if (EditorUserBuildSettings.selectedBuildTargetGroup != buildTargetGroup)
        {
            if (EditorUtility.DisplayDialog("Change Build Platform", "Are you sure you want to change the build platform to " + BuildPlatform + "?. This might take a while.", "Yes", "No"))
            {
                prevBuildPlatform = BuildPlatform;
                bool result = EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroup, buildTarget);
                if (result)
                {
                    Debug.LogFormat("<color=green>Succesfully</color> switched to {0}", BuildPlatform);
                }
                else
                {
                    Debug.LogErrorFormat("<color=red>Failed</color> to switch to {0}", BuildPlatform);
                    return;
                }
            }
            else
            {
                BuildPlatform = prevBuildPlatform;
            }

            EditorUserBuildSettings.selectedBuildTargetGroup = buildTargetGroup;
            UpdateScriptDefineSymbolsList(buildTargetGroup);
        }

        if (BuildPlatform == BuildPlatform.Android)
        {
            switch (AndroidDevice)
            {
                case AndroidDevice.Oculus:
                    PlayerSettings.Android.blitType = AndroidBlitType.Never;
                    ReplaceManifest(oculusManifest, androidManifestDirectory);
                    AddDirective(ScriptingDefineSymbols.OCULUS);
                    break;

                case AndroidDevice.Pico:
                    PlayerSettings.Android.blitType = AndroidBlitType.Auto;
                    ReplaceManifest(picoManifest, androidManifestDirectory);
                    AddDirective(ScriptingDefineSymbols.PICO);
                    break;
                default:
                    Debug.LogError(BuildPlatform + " is not implemented!");
                    break;
            }

        }
        XRGeneralSettings.Instance.InitManagerOnStart = BuildPlatform == BuildPlatform.Android;
    }

    private void AddDirective(string directive)
    {
        if (!AllDirectives.Contains(directive))
            AllDirectives.Add(directive);
    }

    private static void UpdateScriptDefineSymbolsList(BuildTargetGroup buildTargetGroup)
    {
        string allDirectivesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
        if (string.IsNullOrWhiteSpace(allDirectivesString))
            AllDirectives = new List<string>();
        else
            AllDirectives = new List<string>(allDirectivesString.Split(';'));
    }

    private void RemovePlatformSpecificDefineSymbols()
    {
        foreach (string define in allPlatformDirectives)
        {
            if (AllDirectives.Contains(define))
            {
                AllDirectives.Remove(define);
            }
        }
    }

    private void UpdateScriptingDefineSymbolsToBuildTarget(BuildTargetGroup buildTargetGroup)
    {
        if (AllDirectives.Count > 0)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(AllDirectives[0]);
            for (int i = 1; i < AllDirectives.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(AllDirectives[i]))
                {
                    stringBuilder.Append(';');
                    stringBuilder.Append(AllDirectives[i]);
                }
                else
                {
                    AllDirectives.RemoveAt(i--);
                }
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, stringBuilder.ToString());
        }
        else
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, "");
        }
    }

    private static void ReplaceManifest(string from, string to)
    {
        try
        {
            File.Delete(currentAndroidManifest);
            File.Copy(from, to);
        }
        catch(System.Exception e)
        {
            Debug.LogError(e);
        }
    }
}