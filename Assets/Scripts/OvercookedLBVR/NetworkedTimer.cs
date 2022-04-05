using Photon.Pun;
using System;

public class NetworkedTimer : Timer
{
    public double NetworkedTimeStarted { get; private set; }

    public void Start(float startFrom, Action onDoneEvent = null)
    {
        Start(onDoneEvent);
        ElapsedTime = startFrom;
        NetworkedTimeStarted = PhotonNetwork.Time;
    }
}
