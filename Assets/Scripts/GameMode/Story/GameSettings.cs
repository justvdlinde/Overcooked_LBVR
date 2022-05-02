using UnityEngine;

public class GameSettings : ScriptableObject
{
    [Tooltip("Delay between old order removed and new order showing on the displayboard")]
    public float nextOrderDelay = 1;

    [Tooltip("Amount of orders")]
    public int orderAmount = 10;
}
