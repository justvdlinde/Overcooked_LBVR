using UnityEngine;
using UnityEngine.XR;

public class XRInputDebugMenu : IDebugMenu
{
    public void Open() {}
    public void Close() { }

    public void OnGUI(bool drawDeveloperOptions)
    {
        GUILayout.BeginHorizontal("box", GUILayout.MinWidth(200));
        DrawControllerInfo(Hand.Left);
        GUILayout.Space(10);
        DrawControllerInfo(Hand.Right);
        GUILayout.EndHorizontal();
    }

    private void DrawControllerInfo(Hand hand)
    {
        InputDevice controller = hand == Hand.Right ? XRInput.RightController : XRInput.LeftController;
        string title = hand == Hand.Right ? "Right Controller" : "Left Controller";

        GUILayout.BeginVertical(title, "window", GUILayout.MinWidth(300));
        GUILayout.Label("Controller: " + controller != null ? controller.characteristics.ToString() : "Not Found");

        if (controller != null)
        {
            GUILayout.Label("Trigger pressed: " + XRInput.TriggerButtonIsPressed(hand));
            GUILayout.Label("Trigger value: " + XRInput.GetTriggerButtonValue(hand));
            GUILayout.Label("Grip Pressed: " + XRInput.GripButtonIsPressed(hand));
            GUILayout.Label("Grip value: " + XRInput.GetGripButtonValue(hand));
            GUILayout.Label("Primary button pressed: " + XRInput.GetPrimaryButtonPressed(hand));
            GUILayout.Label("Primary button down: " + XRInput.GetPrimaryButtonDown(hand));
            GUILayout.Label("Primary button up: " + XRInput.GetPrimaryButtonUp(hand));
            GUILayout.Label("Secondary button pressed: " + XRInput.GetSecondaryButtonPressed(hand));
            GUILayout.Label("Secondary button down: " + XRInput.GetSecondaryButtonDown(hand));
            GUILayout.Label("Secondary button up: " + XRInput.GetSecondaryButtonUp(hand));
            GUILayout.Label("Thumb stick axis: " + XRInput.GetThumbStickAxis(hand));
            GUILayout.Label("Thumb stick touched: " + XRInput.ThumbStickButtonIsTouched(hand));
            GUILayout.Label("Thumb stick pressed: " + XRInput.ThumbStickButtonIsPressed(hand));
            GUILayout.Label("Menu button pressed: " + XRInput.MenuButtonIsPressed(hand));

            if(GUILayout.Button("Play Haptics for 1 second"))
                XRInput.PlayHaptics(hand, 0.5f, 1);
        }
        GUILayout.EndVertical();
    }
}
