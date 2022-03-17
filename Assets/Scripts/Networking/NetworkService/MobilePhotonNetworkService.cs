public class MobilePhotonNetworkService : PhotonNetworkService
{
    protected override void ConnectToSelfHosted(NetworkConfig config)
    {
        if(GamePlatform.GameType == ClientType.Operator)
            ConnectToCloudAndBroadcastRoomName(config);
        else
            ListenForBroadcastAndConnect(config);
    }
}
