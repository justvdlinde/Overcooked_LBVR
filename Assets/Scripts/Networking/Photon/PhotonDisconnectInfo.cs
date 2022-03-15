using Photon.Realtime;

public class PhotonDisconnectInfo : IDisconnectInfo
{
    public int ErrorCode => (int)Cause;

    public string ErrorDescription { get; private set; }

    public readonly DisconnectCause Cause;

    public PhotonDisconnectInfo(DisconnectCause cause)
    {
        Cause = cause;
        ErrorDescription = PhotonDisconnectCauseDescriptions.GetDescription(cause);
    }

    public PhotonDisconnectInfo(DisconnectCause cause, string errorDescription)
    {
        Cause = cause;
        ErrorDescription = errorDescription;
    }
}
