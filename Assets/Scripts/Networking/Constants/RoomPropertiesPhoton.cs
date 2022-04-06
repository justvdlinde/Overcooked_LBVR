using Photon.Pun;

public static class RoomPropertiesPhoton 
{
    public const string GAME_VERSION            = "GameVersion";
    public const string GAME_MODE               = "GameMode";
	public const string GAME_STATE              = "State";
    public const string GAME_CURRENT_ROUND_NR   = "RoundNr";
    public const string GAME_TIME_STAMP         = "GameTimeStamp";

	public const string CALIBRATION_POSITION_X  = "CalibrationPositionX";
	public const string CALIBRATION_POSITION_Z	= "CalibrationPositionZ";

	public const string PLAYER_ONE_HANDED       = "OneHanded";
    public const string SCENE                   = PhotonNetwork.CurrentSceneProperty;

    public const string MATCH_START_TIME        = "MatchStartTime";
    public const string MATCH_DURATION          = "MatchDuration";
    public const string OBJECTIVE_TARGET        = "ObjectiveTarget";

    public const string TEAM_ONE_SCORE          = "TeamOneScore";
	public const string TEAM_TWO_SCORE = "TeamTwoScore";

    public const string SELECTION_IS_ACTIVE = "SelectionIsActive";
    public const string SELECTION_TYPE = "SelectionType";
    public const string SELECTION_PLAYER_READY_AMOUNT = "SelectionPlayerReadyAmount";
    public const string SELECTION_POPULAR_CHOICE = "SelectionPopularChoice";
    public const string SELECTION_REQUIRES_VOLUME = "SelectionRequiresVolume";
    public const string SELECTION_USES_CANVAS = "SelectionUsesCanvas";
}
