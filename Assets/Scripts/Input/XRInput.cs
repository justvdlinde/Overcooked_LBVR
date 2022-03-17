using System.Collections.Generic;
using Unity.XR.Oculus;
using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// Generic way for doing player input on VR devices
/// Full list of input mappings and devices: https://docs.unity3d.com/Manual/xr_input.html
/// Note: Device specific mappings such as Oculus can be acquired using <see cref="OculusUsages"/> instead of <see cref="CommonUsages"/>
/// </summary>
public static class XRInput
{
    public static InputDevice LeftController { get; private set; }
    public static InputDevice RightController { get; private set; }

    static XRInput()
    {
        List<InputDevice> inputDevices = new List<InputDevice>();

        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, inputDevices);
        for (int i = 0; i < inputDevices.Count; i++)
        {
            OnDeviceConnected(inputDevices[i]);
        }

        InputDevices.deviceConnected += OnDeviceConnected;
        InputDevices.deviceDisconnected += OnDeviceDisconnected;
    }

    private static void OnDeviceConnected(InputDevice device)
    {
        if (device.characteristics.HasFlag(InputDeviceCharacteristics.Left))
            LeftController = device;
        else if (device.characteristics.HasFlag(InputDeviceCharacteristics.Right))
            RightController = device;
    }

    private static void OnDeviceDisconnected(InputDevice device)
    {
        if (device.characteristics.HasFlag(InputDeviceCharacteristics.Left) && LeftController != null)
            LeftController = device;
        else if (device.characteristics.HasFlag(InputDeviceCharacteristics.Right) && RightController != null)
            RightController = device;
    }

    public static bool TriggerButtonIsPressed(Hand hand)
    {
        InputDevice controller = hand == Hand.Right ? RightController : LeftController;
        if (controller == null)
            return false;
        controller.TryGetFeatureValue(CommonUsages.triggerButton, out bool value);
        return value;
    }

    public static float GetTriggerButtonValue(Hand hand)
    {
        InputDevice controller = hand == Hand.Right ? RightController : LeftController;
        if (controller == null)
            return 0;
        controller.TryGetFeatureValue(CommonUsages.trigger, out float value);
        return value;
    }

    public static bool GripButtonIsPressed(Hand hand)
    {
        InputDevice controller = hand == Hand.Right ? RightController : LeftController;
        if (controller == null)
            return false;
        controller.TryGetFeatureValue(CommonUsages.gripButton, out bool value);
        return value;
    }

    public static float GetGripButtonValue(Hand hand)
    {
        InputDevice controller = hand == Hand.Right ? RightController : LeftController;
        if (controller == null)
            return 0;
        controller.TryGetFeatureValue(CommonUsages.grip, out float value);
        return value;
    }

    public static bool GetPrimaryButtonPressed(Hand hand)
    {
        InputDevice controller = hand == Hand.Right ? RightController : LeftController;
        if (controller == null)
            return false;
        controller.TryGetFeatureValue(CommonUsages.primaryButton, out bool value);
        return value;
    }

    public static bool GetSecondaryButtonPressed(Hand hand)
    {
        InputDevice controller = hand == Hand.Right ? RightController : LeftController;
        if (controller == null)
            return false;
        controller.TryGetFeatureValue(CommonUsages.secondaryButton, out bool value);
        return value;
    }

    public static Vector2 GetThumbStickAxis(Hand hand)
    {
        InputDevice controller = hand == Hand.Right ? RightController : LeftController;
        if (controller == null)
            return Vector2.zero;
        controller.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 value);
        return value;
    }

    public static bool ThumbStickButtonIsPressed(Hand hand)
    {
        InputDevice controller = hand == Hand.Right ? RightController : LeftController;
        if (controller == null)
            return false;
        controller.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool value);
        return value;
    }

    public static bool ThumbStickButtonIsTouched(Hand hand)
    {
        InputDevice controller = hand == Hand.Right ? RightController : LeftController;
        if (controller == null)
            return false;
        controller.TryGetFeatureValue(CommonUsages.primary2DAxisTouch, out bool value);
        return value;
    }

    public static bool MenuButtonIsPressed(Hand hand)
    {
        InputDevice controller = hand == Hand.Right ? RightController : LeftController;
        if (controller == null)
            return false;
        controller.TryGetFeatureValue(CommonUsages.menuButton, out bool value);
        return value;
    }

    public static bool PlayHaptics(Hand hand, float strength, float durationInSeconds)
    {
        InputDevice controller = hand == Hand.Right ? RightController : LeftController;
        if (controller == null)
            return false;

        if (controller.TryGetHapticCapabilities(out HapticCapabilities capabilities))
        {
            if (capabilities.supportsImpulse)
            {
                uint channel = 0;
                strength = Mathf.Clamp01(strength);
                return controller.SendHapticImpulse(channel, strength, durationInSeconds);
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
}
