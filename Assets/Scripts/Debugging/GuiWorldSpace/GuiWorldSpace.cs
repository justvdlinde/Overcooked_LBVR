using System;

public static class GUIWorldSpace
{
    /// <summary>
    /// Logs text to the <see cref="GuiWorldSpacePanel"/>, call in Update()
    /// </summary>
    /// <param name="text"></param>
    public static void Log(string text)
    {
        if (GuiWorldSpacePanel.Instance != null)
            GuiWorldSpacePanel.Instance.AddLog(text);
    }

    public static GuiWorldSpacePanelButton AddButton(string title, Action onClick)
    {
        if (GuiWorldSpacePanel.Instance != null)
            return GuiWorldSpacePanel.Instance.AddButton(title, onClick);
        else
            return null;
    }

    public static bool RemoveButton(GuiWorldSpacePanelButton button)
    {
        if (GuiWorldSpacePanel.Instance != null)
            return GuiWorldSpacePanel.Instance.RemoveButton(button);
        else
            return false;
    }
}