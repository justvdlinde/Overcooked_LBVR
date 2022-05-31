using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneOrderDisplayManager : OrderDisplayManager
{
    private static SceneOrderDisplayManager Instance;

    protected override void Awake()
    {
        if (Instance != null)
            Destroy(Instance.gameObject);
        Instance = this;

        base.Awake();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public static bool AllDisplaysAreFree()
    {
        if (Instance == null)
        {
            Debug.LogError("No OrderDisplayManager in scene!");
            return false;
        }

        for (int i = 0; i < Instance.OrderDisplays.Length; i++)
        {
            if (!Instance.OrderDisplays[i].CanBeUsed())
                return false;
        }
        return true;
    }

    public static bool HasFreeDisplay()
    {
        return HasFreeDisplay(out _);
    }

    public static bool HasFreeDisplay(out OrderDisplay display)
    {
        display = GetFreeDisplay();
        return display != null;
    }

    public static OrderDisplay GetFreeDisplay()
    {
        if (Instance == null)
        {
            Debug.LogError("No OrderDisplayManager in scene!");
            return null;
        }

        foreach (OrderDisplay display in Instance.orderDisplays)
        {
            if (display.CanBeUsed())
                return display;
        }
        return null;
    }

}
