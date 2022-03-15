using UnityEngine;

public class NetworkConfig : ScriptableObject
{
    [SerializeField] public ConnectionType connectionType = ConnectionType.CloudLocalRoom;

    [Tooltip("Room name used only when using a connection type of Cloud")]
    [SerializeField] public string roomName = "Room 1";

    [SerializeField] public int serverConnectionAttemptLimit = 30;

    [SerializeField] public int clientConnectionAttemptLimit = 30;
}
