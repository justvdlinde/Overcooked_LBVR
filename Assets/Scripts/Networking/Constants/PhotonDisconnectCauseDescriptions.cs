using Photon.Realtime;

public static class PhotonDisconnectCauseDescriptions 
{
    public const string DEFAULT = "Lost connection to the server. Make sure you have a server running and you're on the same network.";
    public const string MAX_CCU_REACHED = "Maximum number of Concurrent Users detected. Please check your license.";
    public const string UNABLE_TO_FIND_SERVER = "Unable to find a server. Make sure you have a server running and you're on the same network.";
    public const string VERSION_MISMATCH = "Your game version does not match the version on the server, please update your game.";
    public const string MAX_OPERATORS = "An operator is already in-game, please disconnect that operator first.";

    public static string GetDescription(DisconnectCause cause)
    {
        switch (cause)
        {
            case DisconnectCause.MaxCcuReached:
                return MAX_CCU_REACHED;
            case DisconnectCause.ServerUDPSearchTimeout:
                return UNABLE_TO_FIND_SERVER;
            case DisconnectCause.GameVersionMismatch:
                return VERSION_MISMATCH;
            case DisconnectCause.MaxOperatorCountReached:
                return MAX_OPERATORS;
            default:
                return DEFAULT;
        }
    }
}
