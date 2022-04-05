using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

	public static bool IsSelectionActive = false;
	public static bool SelectionRequiresVolume = true;

	private int ReadyPlayerCount = 0;
	private int PopularChoice = -1;

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
		if (pawn == null)
		{
			// some error show
		}
	}

	

	private void OnDisable()
	{
#if GAME_CLIENT
		globalEventDispatcher.Unsubscribe<TurnkeySelectionEvent>(OnSelectionEvent);
		// left empty intentionally
#endif
		IsSelectionActive = false;
		PhotonNetworkService.RoomPropertiesChangedEvent -= OnRoomPropertiesChangedEvent;
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

	#region Selection handling

	private void OnRoomPropertiesChangedEvent(ExitGames.Client.Photon.Hashtable obj)
	{
		if (obj.TryGetValue(RoomPropertiesPhoton.SELECTION_IS_ACTIVE, out object selectionActive))
		{
			IsSelectionActive = (bool)selectionActive;
		}

		if (obj.TryGetValue(RoomPropertiesPhoton.SELECTION_PLAYER_READY_AMOUNT, out object amt))
		{
			ReadyPlayerCount = (int)amt;

			if(PhotonNetwork.IsMasterClient)
			{
				// check coice if ready count equals total player count
				// execute result
				// disable selection
			}

		}

		if (obj.TryGetValue(RoomPropertiesPhoton.SELECTION_TYPE, out object type))
		{
			selectionType = (SelectionType)type;
		}
		if (obj.TryGetValue(RoomPropertiesPhoton.SELECTION_POPULAR_CHOICE, out object p))
		{
			PopularChoice = (int)p;
		}

		if (obj.TryGetValue(RoomPropertiesPhoton.SELECTION_REQUIRES_VOLUME, out object r))
		{
			SelectionRequiresVolume = (bool)r;
		}
	}

	private void HandleSelectionReadyEvent(bool useDefault = false)
	{
		int choice = (useDefault) ? 0 : PopularChoice;

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

	private void HandleGameModeSelection(int selectedVolume)
	{
		// set some gamemode event that calls the event like an operator UI would
	}

	private void HandleReplaySelection(int selectedVolume)
	{
		GameMode gm = GlobalServiceLocator.Instance.Get<GameModeService>().CurrentGameMode;
		// depending on selectedVolume handle some callback
		switch (selectedVolume)
		{
			case 1:
				// call replay event across clients
				gm.Replay();
				break;
			default:
				// go to other place
				break;
		}
	}

	private void HandleReadyUpSelection(int selectedVolume)
	{
		GameMode gm = GlobalServiceLocator.Instance.Get<GameModeService>().CurrentGameMode;
		// depending on selectedVolume handle some callback
		switch (selectedVolume)
		{
			case 1:
				// call replay event across clients
				gm.StartActiveGame();
				break;
			default:
				// go to other place
				break;
		}
	}

	private void HandleMapSelection(int selectedVolume)
	{
		// depending on selectedVolume handle some callback
	}
	#endregion

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
		if (!IsSelectionActive)
			StartSelection(selectionType);
		else
			StopSelection();
	}

	[Button]
	public void SetRequiresVolume()
	{
		PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.SELECTION_REQUIRES_VOLUME, !SelectionRequiresVolume);
	}

	public void StartSelection(SelectionType type)
	{
		PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.SELECTION_IS_ACTIVE, true);
		PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.SELECTION_TYPE, (int)type);
	}

	public void StopSelection()
	{
		PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.SELECTION_IS_ACTIVE, false);
		PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.SELECTION_TYPE, 0);
	}

	private void OnSelectionEvent(TurnkeySelectionEvent obj)
	{
		currentPlayerEventState = obj;

		if (PhotonNetwork.LocalPlayer.IsLocal)
		{
			PhotonNetwork.LocalPlayer.SetCustomProperty(PlayerPropertiesPhoton.SELECTION_PLAYER_VOLUME_ID, obj.volumeID);
		}
	}

	[PunRPC]
	private void SelectionEventRPC(PhotonMessageInfo info)
	{
		if (PhotonNetwork.IsMasterClient)
		{
			CountReadyPlayers();
		}
	}


	private int CountReadyPlayers()
	{
		int playerCount = PhotonNetwork.PlayerList.Length;

		List<int> choices = new List<int>();

		int readyCount = 0;
		foreach (var item in PhotonNetwork.PlayerList)
		{
			bool playerIsInVolume = false;
			if (item.CustomProperties.TryGetValue(PlayerPropertiesPhoton.SELECTION_PLAYER_VOLUME_ID, out object p))
			{
				playerIsInVolume = (int)p != -1;

				if ((int)p >= 0)
					choices.Add((int)p);
			}

			if (!playerIsInVolume && SelectionRequiresVolume)
				continue;

			bool playerIsReady = false;
			if (item.CustomProperties.TryGetValue(PlayerPropertiesPhoton.SELECTION_PLAYER_IS_READY, out object r))
				playerIsReady = (bool)r;

			// some way to find what volume is popular (could be handled locally)

			if (playerIsReady && (playerIsInVolume || !SelectionRequiresVolume))
				readyCount++;
		}

		if (PhotonNetwork.IsMasterClient)
		{
			if (choices.Count > 0)
				PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.SELECTION_POPULAR_CHOICE, Mode<int>(choices));
			else
				PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.SELECTION_POPULAR_CHOICE, -1);

			PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.SELECTION_PLAYER_READY_AMOUNT, readyCount);
		}

		return readyCount;
	}


	private void RaisePhotonEventCode(int code, object[] content = null, ReceiverGroup receivers = ReceiverGroup.Others)
	{
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = receivers };
		PhotonNetwork.RaiseEvent((byte)code, content, raiseEventOptions, SendOptions.SendReliable);
	}


	#region Display functionality
	private void SetTexts()
	{
		string text = "";

		if (!IsSelectionActive || selectionType == SelectionType.None)
		{
			ApplyTexts("");
			return;
		}

		text += GetTextForSelectionType(selectionType) + "\n";

		if (PopularChoice == -1)
			text += "No popular choice";
		else
			text += "Popular choice is: " + ((PopularChoice == 0) ? "No" : "Yes");
		text += "\n";

		int playerCount = PhotonNetwork.PlayerList.Length;

		// get this from photon room properties
		text += $"{ReadyPlayerCount}/{playerCount} players ready";

		ApplyTexts(text);
	}

	public string GetTextForSelectionType(SelectionType type)
	{
		switch (type)
		{
			default:
				return "";
			case SelectionType.Gamemode:
				return "Gamemode selection";
			case SelectionType.Replay:
				return "Do you want to play another game?";
			case SelectionType.MapSelection:
				return "Map selection";
			case SelectionType.Gameplay:
				return "Do you want to start the game?";
			case SelectionType.ReadyUp:
				return "Are you ready to play?";
		}
	}

	private void ApplyTexts(string text)
	{
		foreach (var item in texts)
		{
			item.text = text;
		}
	}
	#endregion

	#region ClientSide ready functionality

	public bool IsPawnInSelectionVolume()
	{
		if (!SelectionRequiresVolume)
			return true;

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

		if (IsPawnReady && PhotonNetwork.LocalPlayer.IsLocal)
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

		if (pawn.IsPawnGesturingReady(Hand.Left))
			XRInput.PlayHaptics(Hand.Left, 0.8f, 0.3f);
		if (pawn.IsPawnGesturingReady(Hand.Right))
			XRInput.PlayHaptics(Hand.Right, 0.8f, 0.3f);

		yield return new WaitForSeconds(0.4f);

		if (pawn.IsPawnGesturingReady(Hand.Left))
			XRInput.PlayHaptics(Hand.Left, 0.8f, 0.3f);
		if (pawn.IsPawnGesturingReady(Hand.Right))
			XRInput.PlayHaptics(Hand.Right, 0.8f, 0.3f);
	}
	#endregion

	public T Mode<T>(IEnumerable<T> list)
	{
		// Initialize the return value
		T mode = default(T);

		// Test for a null reference and an empty list
		if (list != null && list.Count() > 0)
		{
			// Store the number of occurences for each element
			Dictionary<T, int> counts = new Dictionary<T, int>();

			// Add one to the count for the occurence of a character
			foreach (T element in list)
			{
				if (counts.ContainsKey(element))
					counts[element]++;
				else
					counts.Add(element, 1);
			}

			// Loop through the counts of each element and find the 
			// element that occurred most often
			int max = 0;

			foreach (KeyValuePair<T, int> count in counts)
			{
				if (count.Value > max)
				{
					// Update the mode
					mode = count.Key;
					max = count.Value;
				}
			}
		}

		return mode;
	}
}
