using UnityEngine;

public class GameModeSettings : ScriptableObject
{
    public float countdownDuration = 3;

    public int scoreTarget = 10;

    [Tooltip("Time in seconds the match will last")]
    public float matchDuration = 120;
}
