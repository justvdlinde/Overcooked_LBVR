using Photon.Pun;
using UnityEngine;

public class OperatorFlow : GameFlow
{
    public Operator Operator { get; private set; }

    protected override void Awake()
    {
        CreateOperator();
        base.Awake();
        ConnectToNetwork();
    }

    protected virtual void CreateOperator()
    {
        OperatorFactory factory = new OperatorFactory(injector, PhotonNetwork.LocalPlayer);
        Operator = factory.Construct();
        networkService.AddClient(Operator);
    }

    protected override void OnConnectionSuccessEvent(ConnectionSuccessEvent @event)
    {
        Debug.Log("OnConnectionSuccessEvent");

        // If an operator is already present, disconnect
        //if (playersManager.Operators.Count > 1)
        //{
        //    networkService.Disconnect((new PhotonDisconnectInfo(Photon.Realtime.DisconnectCause.MaxOperatorCountReached)));
        //    return;
        //}
        
        PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
        base.OnConnectionSuccessEvent(@event);
    }
}