using System.Collections.Generic;

public static class PlayerPropertiesPhoton 
{
    public const string PLAYER_NAME         = "Name";
    public const string IP_ADDRESS          = "IPAddress";
    public const string DEVICE_ID           = "MacAddress";
    public const string CLIENT_TYPE         = "ClientType";
	public const string GAME_VERSION		= "GameVersion";

	public const string STATE               = "State";
    public const string HEALTH_POINTS       = "HP";
    public const string TEAM                = "Team";

    public const string LEFT_HAND_ITEM_NAME     = "LeftItemName";
    public const string LEFT_HAND_ITEM_VIEW_ID  = "LeftItemID";
    public const string RIGHT_HAND_ITEM_NAME    = "RightItemName";
    public const string RIGHT_HAND_ITEM_VIEW_ID = "RightItemID";


    public static readonly Dictionary<Hand, string> HandItemNamePropertyPair = new Dictionary<Hand, string>()
    {
        { Hand.Left, LEFT_HAND_ITEM_NAME },
        { Hand.Right, RIGHT_HAND_ITEM_NAME }
    };

    public static readonly Dictionary<Hand, string> HandViewIdPropertyPair = new Dictionary<Hand, string>()
    {
        { Hand.Left, LEFT_HAND_ITEM_VIEW_ID },
        { Hand.Right, RIGHT_HAND_ITEM_VIEW_ID }
    };
}
