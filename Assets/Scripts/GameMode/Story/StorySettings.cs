using UnityEngine;

public class StorySettings : GameSettings
{
    [Tooltip("Duration of game in seconds")]
    public float gameDuration = 180;

    [Tooltip("Initial starting delay on game start before first order")]
    public float initialStartDelay = 1;

    [Tooltip("Min delay between new order")]
    public float orderDelayMin = 5;

    [Tooltip("Max delay between new order")]
    public float orderDelayMax = 10;
}
