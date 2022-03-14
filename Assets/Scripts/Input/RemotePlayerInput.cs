using UnityEngine;

/// <summary>
/// Remote player input, not all values are set as these are not used on remote player pawns
/// </summary>
public class RemotePlayerInput 
{
    public float TriggerButtonRight { get; set; }
    public float TriggerButtonLeft { get; set; }
    public float GripButtonValueRight { get; set; }
    public float GripButtonValueLeft { get; set; }
    public bool ThumbstickIsTouchedRight { get; set; }
    public bool ThumbstickIsTouchedLeft { get; set; }

    public float GetGripButtonValue(Hand hand)
    {
        return hand == Hand.Right ? GripButtonValueRight : GripButtonValueLeft;
    }
    public bool GripButtonIsPressed(Hand hand)
    {
        return (hand == Hand.Right ? GripButtonValueRight : GripButtonValueLeft) > 0.95f;
    }

    public Vector2 GetThumbstickAxis(Hand hand)
    {
        return Vector2.zero;
    }

    public float GetTriggerButtonValue(Hand hand)
    {
        return hand == Hand.Right ? TriggerButtonRight : TriggerButtonLeft;
    }

    public bool TriggerButtonIsPressed(Hand hand)
    {
        return (hand == Hand.Right ? GripButtonValueRight : GripButtonValueLeft) > 0.95f;
    }

    public bool GetPrimaryButtonPressed(Hand hand)
    {
        return false;
    }

    public bool GetSecondaryButtonPressed(Hand hand)
    {
        return false;
    }

    public bool ThumbStickButtonIsTouched(Hand hand)
    {
        return hand == Hand.Right ? ThumbstickIsTouchedRight : ThumbstickIsTouchedLeft;
    }
}
