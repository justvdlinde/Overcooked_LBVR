using Photon.Pun;
using System;

public class NetworkedTimer : Timer
{
    public double NetworkedTimeStarted { get; private set; } = 0;

    public void Set(double networkTimeStarted, float durationInSeconds)
    {
        base.Set(durationInSeconds);
        NetworkedTimeStarted = networkTimeStarted;
    }

    public override void Start(Action onDoneEvent = null, float elapsed = 0)
    {
        base.Start(onDoneEvent, elapsed);

        if (NetworkedTimeStarted != 0)
            ElapsedTime = (float)(PhotonNetwork.Time - NetworkedTimeStarted);
        else
            NetworkedTimeStarted = PhotonNetwork.Time;
    }
}
