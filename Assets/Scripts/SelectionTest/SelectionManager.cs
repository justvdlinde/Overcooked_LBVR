using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Attributes;
using Utils.Core.Events;
using Utils.Core.Services;
using static TMPro.TMP_InputField;



// needs some sort of network sync.
// main client (server/operator) needs the managing role and invoke operator-type commands such as
// level select, gamemode select, game start, game stop
public class SelectionManager : MonoBehaviourPun
{
    private SelectionPawn pawn = null;

	public bool IsSelectionActive = false;

	private int ReadyPlayerCount = 0;

	public static bool IsPawnReady = false;

	public float timeToHoldGestureToRegisterReady = 5f;
	private float timeGestureHeld = 0.0f;

	private GlobalEventDispatcher globalEventDispatcher = null;

	private TurnkeySelectionEvent currentPlayerEventState = null;

	[SerializeField] private List<TMPro.TextMeshProUGUI> texts = null;

	[SerializeField] public SelectionType selectionType = SelectionType.None;

	private void OnEnable()
	{
		pawn = Object.FindObjectOfType<SelectionPawn>();

		globalEventDispatcher = GlobalServiceLocator.Instance.Get<GlobalEventDispatcher>();

#if GAME_CLIENT
		globalEventDispatcher.Subscribe<TurnkeySelectionEvent>(OnSelectionEvent);
#endif
		PhotonNetworkService.RoomPropertiesChangedEvent += OnRoomPropertiesChangedEvent;
		if(pawn == null)
		{
			// some error show
		}

		RequestSelectionStateRPC();
	}

	private void OnRoomPropertiesChangedEvent(ExitGames.Client.Photon.Hashtable obj)
	{
        if (obj.TryGetValue(RoomPropertiesPhoton.SELECTION_IS_ACTIVE, out object selectionActive))
		{
			IsSelectionActive = (bool)selectionActive;
		}

		if (obj.TryGetValue(RoomPropertiesPhoton.SELECTION_PLAYER_READY_AMOUNT, out object amt))
		{
			ReadyPlayerCount = (int)amt;
		}
	}

	private void OnDisable()
	{
#if GAME_CLIENT
		globalEventDispatcher.Unsubscribe<TurnkeySelectionEvent>(OnSelectionEvent);
		// left empty intentionally
#endif
		PhotonNetworkService.RoomPropertiesChangedEvent -= OnRoomPropertiesChangedEvent;
	}



	private void HandleSelectionReadyEvent()
	{
		switch (selectionType)
		{
			case SelectionType.None:
				return;
			case SelectionType.Gamemode:
				//HandleGameModeSelection();
				break;
			case SelectionType.Replay:
				//globalEventDispatcher.Invoke<ReplayEvent>(new ReplayEvent());
				break;
			case SelectionType.MapSelection:
				break;
			case SelectionType.Gameplay:
				break;
			case SelectionType.ReadyUp:
				break;
		}
	}

	//private void HandleGameModeSelection(int selectedVolume)
	//{
	//	// set some gamemode event that calls the event like an operator UI would
	//}

	//private void HandleReplaySelection(int selectedVolume)
	//{
	//	// depending on selectedVolume handle some callback
	//}

	//private void HandleReplaySelection(int selectedVolume)
	//{
	//	// depending on selectedVolume handle some callback
	//}

	// debug
	[Button]
	private void TransferOwnership()
	{
		photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
		PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
	}

	[Button]
	public void ToggleSelection()
	{
		PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.SELECTION_IS_ACTIVE, !IsSelectionActive);

		//photonView.RPC("ToggleSeletionRPC", RpcTarget.All);
	}

	public void RequestSelectionState()
	{
		photonView.RPC("RequestSelectionStateRPC", RpcTarget.MasterClient);

	}

	[PunRPC]
	public void RequestSelectionStateRPC()
	{
		if (PhotonNetwork.IsMasterClient)
			photonView.RPC("ToggleSeletionRPC", RpcTarget.All);
	}

	[PunRPC]
	public void ToggleSeletionRPC(PhotonMessageInfo info)
	{
		if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(RoomPropertiesPhoton.SELECTION_IS_ACTIVE))
			IsSelectionActive = (bool)PhotonNetwork.CurrentRoom.CustomProperties[RoomPropertiesPhoton.SELECTION_IS_ACTIVE];
		//IsSelectionActive = selectionActive;
	}

	private void OnSelectionEvent(TurnkeySelectionEvent obj)
	{
		currentPlayerEventState = obj;

		if(PhotonNetwork.LocalPlayer.IsLocal)
		{
			PhotonNetwork.LocalPlayer.SetCustomProperty(PlayerPropertiesPhoton.SELECTION_PLAYER_VOLUME_ID, obj.volumeID);
			
		}
	}

	[PunRPC]
	private void SelectionEventRPC(PhotonMessageInfo info)
	{
		Debug.Log("selection event");
		if (PhotonNetwork.IsMasterClient)
		{
			CountReadyPlayers();
		}
	}


	private int CountReadyPlayers()
	{
		int playerCount = PhotonNetwork.PlayerList.Length;

		int readyCount = 0;
		foreach (var item in PhotonNetwork.PlayerList)
		{
			bool playerIsInVolume = false;
			if (item.CustomProperties.TryGetValue(PlayerPropertiesPhoton.SELECTION_PLAYER_VOLUME_ID, out object p))
				playerIsInVolume = (int)p != -1;

			Debug.Log(p + " " + playerIsInVolume);

			if (!playerIsInVolume)
				continue;

			bool playerIsReady = false;
			if (item.CustomProperties.TryGetValue(PlayerPropertiesPhoton.SELECTION_PLAYER_IS_READY, out object r))
				playerIsReady = (bool)r;

			// some way to find what volume is popular (could be handled locally)

			if (playerIsReady && playerIsInVolume)
				readyCount++;
		}

		if(PhotonNetwork.IsMasterClient)
		{
			PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.SELECTION_PLAYER_READY_AMOUNT, readyCount);
		}

		return readyCount;
	}

	private void SetTexts()
	{
		string text = "";

		if (IsSelectionActive)
			text += "Selection is active \n";
		else
		{
			text += "Selection is inactive";
			ApplyTexts(text);
			return;
		}

		text += "Voting for: " + selectionType.ToString() + "\n";

		if (IsPawnInSelectionVolume())
			text += "Player is in volume: " + currentPlayerEventState.volumeID + "\n";
		else
			text += "Player is not in volume \n";

		int playerCount = PhotonNetwork.PlayerList.Length;

		// get this from photon room properties
		text += $"{ReadyPlayerCount}/{playerCount} players ready";

		ApplyTexts(text);
	}

	private void ApplyTexts(string text)
	{
		foreach (var item in texts)
		{
			item.text = text;
		}
	}

	private void Update()
	{
		if (pawn == null)
			return; // show some error

		SetTexts();

		if (IsSelectionActive)
		{
			if (IsPawnInSelectionVolume())
			{
				if (pawn.IsPawnGesturingReady())
				{
					if (timeGestureHeld < timeToHoldGestureToRegisterReady)
					{
						timeGestureHeld += Time.deltaTime;

						if (pawn.IsPawnGesturingReady(Hand.Left))
							XRInput.PlayHaptics(Hand.Left, Mathf.Lerp(0.05f, 0.5f, Mathf.InverseLerp(0, timeToHoldGestureToRegisterReady, timeGestureHeld)), Time.deltaTime);
						if (pawn.IsPawnGesturingReady(Hand.Right))
							XRInput.PlayHaptics(Hand.Right, Mathf.Lerp(0.05f, 0.5f, Mathf.InverseLerp(0, timeToHoldGestureToRegisterReady, timeGestureHeld)), Time.deltaTime);
					}

					if (timeGestureHeld >= timeToHoldGestureToRegisterReady)
						SetReadyToTrue();
				}
				else
					SetReadyToFalse();
			}
			else
				SetReadyToFalse();
		}
	}

	private bool IsPawnInSelectionVolume()
	{
		if (currentPlayerEventState == null)
			return false;
		else
			return currentPlayerEventState.isInVolume;
	}

	private void SetReadyToTrue()
	{
		if (!IsPawnReady)
		{
			isReadyBuzzRoutine = StartCoroutine(BuzzRoutine());
			if (!IsPawnReady && PhotonNetwork.LocalPlayer.IsLocal)
			{
				PhotonNetwork.LocalPlayer.SetCustomProperty(PlayerPropertiesPhoton.SELECTION_PLAYER_IS_READY, true);
				photonView.RPC("SelectionEventRPC", RpcTarget.MasterClient);
			}
		}
		IsPawnReady = true;
		// some event call
	}

	private void SetReadyToFalse()
	{
		if (isReadyBuzzRoutine != null && IsPawnReady)
			StopCoroutine(isReadyBuzzRoutine);

		if(IsPawnReady && PhotonNetwork.LocalPlayer.IsLocal)
		{
			PhotonNetwork.LocalPlayer.SetCustomProperty(PlayerPropertiesPhoton.SELECTION_PLAYER_IS_READY, false);
			photonView.RPC("SelectionEventRPC", RpcTarget.MasterClient);
		}

		timeGestureHeld = 0.0f;
		IsPawnReady = false;
		// some event call
	}
	

	private Coroutine isReadyBuzzRoutine = null;
	// translate this to some wave
	private IEnumerator BuzzRoutine()
	{
		yield return new WaitForEndOfFrame();

		if (pawn.IsPawnGesturingReady(Hand.Left))
			XRInput.PlayHaptics(Hand.Left, 0.5f, 0.2f);
		if (pawn.IsPawnGesturingReady(Hand.Right))
			XRInput.PlayHaptics(Hand.Right, 0.5f, 0.2f);

		yield return new WaitForSeconds(0.5f);

		if(pawn.IsPawnGesturingReady(Hand.Left))
			XRInput.PlayHaptics(Hand.Left, 0.8f, 0.3f);
		if (pawn.IsPawnGesturingReady(Hand.Right))
			XRInput.PlayHaptics(Hand.Right, 0.8f, 0.3f);

		yield return new WaitForSeconds(0.4f);

		if (pawn.IsPawnGesturingReady(Hand.Left))
			XRInput.PlayHaptics(Hand.Left, 0.8f, 0.3f);
		if (pawn.IsPawnGesturingReady(Hand.Right))
			XRInput.PlayHaptics(Hand.Right, 0.8f, 0.3f);
	}

}
