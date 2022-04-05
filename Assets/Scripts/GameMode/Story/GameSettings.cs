using UnityEngine;

public class GameSettings : ScriptableObject
{
    [Tooltip("Duration of match in seconds")]
    public float duration = 10;

    [Tooltip("Amount of orders")]
    public int orderAmount = 10;
}
