using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils.Core.Extensions;

/// <summary>
/// Simple world-space panel for displaying debug values, call <see cref="AddLog(string)"/> in an Update function to show a log.
/// Requires to be have a script execution order of less then any script calling <see cref="AddLog(string)"/>.
/// </summary>
public class GuiWorldSpacePanel : MonoBehaviour
{
    public static GuiWorldSpacePanel Instance { get; private set; }
    public Text Label => label;

    [SerializeField] private Text label = null;
    [SerializeField] private GuiWorldSpacePanelButton buttonPrefab = null;
    [SerializeField] private Transform buttonGroup = null;
    [SerializeField] private List<GuiWorldSpacePanelButton> buttons = new List<GuiWorldSpacePanelButton>();

    private List<string> logs = new List<string>();

    private void Awake()
    {
        buttonGroup.RemoveAllChildren();

        if (Debug.isDebugBuild)
            Instance = this;
        else
            gameObject.SetActive(false);
    }

    private void Update()
    {
        ClearLogs();
    }

    private void LateUpdate()
    {
        PrintLogs();
    }

    public void AddLog(string text)
    {
        if (gameObject.activeInHierarchy)
            logs.Add(text);
    }

    private void ClearLogs()
    {
        label.text = string.Empty;
        logs.Clear();
    }

    private void PrintLogs()
    {
        for (int i = 0; i < logs.Count; i++)
        {
            label.text += logs[i] + "\n";
        }
    }

    public GuiWorldSpacePanelButton AddButton(string title, Action onClick)
    {
        GuiWorldSpacePanelButton buttonInstance = Instantiate(buttonPrefab, buttonGroup);
        buttonInstance.title.text = title;
        buttonInstance.button.onClick.AddListener(() => onClick());
        buttons.Add(buttonInstance);
        return buttonInstance;
    }

    public bool RemoveButton(GuiWorldSpacePanelButton button)
    {
        if(buttons.Contains(button))
        {
            Destroy(button.gameObject);
            buttons.Remove(button);
            return true;
        }

        return false;
    }
}

