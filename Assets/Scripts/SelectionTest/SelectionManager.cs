using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils.Core.Attributes;
using Utils.Core.Events;
using Utils.Core.SceneManagement;
using Utils.Core.Services;
using static TMPro.TMP_InputField;



// needs some sort of network sync.
// main client (server/operator) needs the managing role and invoke operator-type commands such as
// level select, gamemode select, game start, game stop
public class SelectionManager : MonoBehaviourPun
{
	public static SelectionManager Instance = null;

	public GameLevel selectionStage = null;

	private SelectionPawn pawn = null;

	public static bool IsSelectionActive = false;
	public static bool SelectionRequiresVolume = true;

	private int ReadyPlayerCount = 0;
	private int PopularChoice = -1;

	private bool SelectionUsesCanvas = true;

	public static bool IsPawnReady = false;

	public float timeToHoldGestureToRegisterReady = 5f;
	private float timeGestureHeld = 0.0f;

	private GlobalEventDispatcher globalEventDispatcher = null;

	private TurnkeySelectionEvent currentPlayerEventState = null;

	[SerializeField] private List<TMPro.TextMeshProUGUI> texts = null;
	[SerializeField] private GameObject canvasObject = null;

	[SerializeField] public SelectionType selectionType = SelectionType.None;

	private SceneService sceneService = null;

	[Header("GameModeSelection maps")]
	[SerializeField] private List<GameLevel> levelSelect = new List<GameLevel>();

	private Dictionary<SelectionType, string[]> selectionVolumeTexts = new Dictionary<SelectionType, string[]>
	{
		{SelectionType.None, new string[2]{"Contact operator", "Contact Operator"} },
		{SelectionType.ReadyUp, new string[2]{"Not ready", "Ready"} },
		{SelectionType.Replay, new string[2]{"Back to main menu", "Play another game" } },
		{SelectionType.MapSelection, new string[2]{"Select map", "Select map"} },
		{SelectionType.Gameplay, new string[2]{"Back to main menu", "Start game", } },
		{SelectionType.Gamemode, new string[3]{"Back to main menu", "Story mode", "Highscore mode"} }
	};

	private void OnEnable()
	{
		Instance = this;

		pawn = GameObject.FindObjectOfType<SelectionPawn>();

		globalEventDispatcher = GlobalServiceLocator.Instance.Get<GlobalEventDispatcher>();
		sceneService = GlobalServiceLocator.Instance.Get<SceneService>();

		DontDestroyOnLoad(gameObject);

#if GAME_CLIENT
		globalEventDispatcher.Subscribe<TurnkeySelectionEvent>(OnSelectionEvent);
#endif
		globalEventDispatcher.Subscribe<ConnectionSuccessEvent>(OnConnectionSuccessEvent);
		globalEventDispatcher.Subscribe<SceneLoadedEvent>(OnSceneLoadedEvent);

		PhotonNetworkService.RoomPropertiesChangedEvent += OnRoomPropertiesChangedEvent;
	}

	private void OnDisable()
	{
#if GAME_CLIENT
		globalEventDispatcher.Unsubscribe<TurnkeySelectionEvent>(OnSelectionEvent);
#endif
		globalEventDispatcher.Unsubscribe<ConnectionSuccessEvent>(OnConnectionSuccessEvent);
		globalEventDispatcher.Unsubscribe<SceneLoadedEvent>(OnSceneLoadedEvent);

		IsSelectionActive = false;
		PhotonNetworkService.RoomPropertiesChangedEvent -= OnRoomPropertiesChangedEvent;
	}

	private void Update()
	{
		if (pawn == null)
		{
			pawn = GameObject.FindObjectOfType<SelectionPawn>();
			if(pawn == null)
				return; // show some error
		}

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

	public bool hasStartedReadyCheck = false;
	private Action onReadyCallback = null;

	public void StartReadyCheck(Action callback)
	{
		Debug.Log("Starting ready check");
		if (hasStartedReadyCheck)
			return;
		hasStartedReadyCheck = true;
		onReadyCallback = callback;
		InitiateSelection(SelectionType.ReadyUp, false, false, true);
	}

	#region Selection handling
	[Tooltip("Short delay before scene starts to actually load, this allows for a screen fade for example")]
	[SerializeField] protected float sceneLoadDelay = 0.1f;
	public void InitiateSelection(SelectionType type, bool useCanvas, bool requiresVolume, bool doneInScene = false)
	{
		initiatedSelectionType = type;
		initiatedUseCanvas = useCanvas;
		initiatedRequiresVolume = requiresVolume;
		sceneService.SceneLoadFinishEvent += OnSceneLoadFinishEvent;
		if(!doneInScene)
			LoadScene(selectionStage.Scene.SceneName);
		//StartCoroutine(LoadSceneDelayed(selectionStage.Scene.SceneName, sceneLoadDelay, () => { StartSelection(type, useCanvas); }));
	}

	private SelectionType initiatedSelectionType = SelectionType.None;
	private bool initiatedUseCanvas = false;
	private bool initiatedRequiresVolume = false;

	private void OnSceneLoadFinishEvent(Scene scene, LoadSceneMode loadMode)
	{
		StartSelection(initiatedSelectionType, initiatedUseCanvas, initiatedRequiresVolume);
		sceneService.SceneLoadFinishEvent -= OnSceneLoadFinishEvent;
	}

	protected virtual void LoadScene(string sceneName)
	{
		if (sceneLoadDelay > 0)
			StartCoroutine(LoadSceneDelayed(sceneName, sceneLoadDelay));
		else
			sceneService.LoadSceneAsync(sceneName);
	}

	protected virtual IEnumerator LoadSceneDelayed(string sceneName, float seconds, Action onDone = null)
	{
		globalEventDispatcher.Invoke(new StartDelayedSceneLoadEvent(sceneName, seconds));
		yield return new WaitForSeconds(seconds);
		onDone?.Invoke();
		sceneService.LoadSceneAsync(sceneName);
	}
	private void OnRoomPropertiesChangedEvent(ExitGames.Client.Photon.Hashtable obj)
	{
		if (obj.TryGetValue(RoomPropertiesPhoton.SELECTION_IS_ACTIVE, out object selectionActive))
		{
			IsSelectionActive = (bool)selectionActive;
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

		if(obj.TryGetValue(RoomPropertiesPhoton.SELECTION_USES_CANVAS, out object c))
		{
			SelectionUsesCanvas = (bool)c;
			canvasObject.SetActive(SelectionUsesCanvas);
		}

		if (obj.TryGetValue(RoomPropertiesPhoton.SELECTION_PLAYER_READY_AMOUNT, out object amt))
		{
			ReadyPlayerCount = (int)amt;

			if (PhotonNetwork.IsMasterClient && ReadyPlayerCount >= PhotonNetwork.PlayerList.Length)
			{
				// check choice if ready count equals total player count
				// execute result
				// disable selection
				HandleSelectionReadyEvent(false);
			}
		}
	}

	private void HandleSelectionReadyEvent(bool useDefault = false)
	{
		int choice = (useDefault) ? 0 : PopularChoice;

		StopSelection();
		switch (selectionType)
		{
			case SelectionType.None:
				return;
			case SelectionType.Replay:
				//globalEventDispatcher.Invoke<ReplayEvent>(new ReplayEvent());
				break;
			case SelectionType.Gamemode:
			case SelectionType.MapSelection:
				HandleMapSelection(choice);
				break;
			case SelectionType.Gameplay:
				break;
			case SelectionType.ReadyUp:
				HandleReadyCheck(choice);
				break;
		}
	}

	private void HandleReadyCheck(int selectedVolume)
	{
		Debug.Log(selectedVolume);
		switch (selectedVolume)
		{
			case 1:
				// call replay event across clients
				onReadyCallback?.Invoke();
				break;
			default:
				Debug.Log("defaulting");
				sceneService.LoadScene(selectionStage.Scene.SceneName);
				// go to other place
				break;
		}
		onReadyCallback = null;

		hasStartedReadyCheck = false;
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
				sceneService.LoadScene(selectionStage.Scene.SceneName);
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
		Debug.Log("People picked choice " + GetTextForVolume(selectedVolume));
		switch (selectedVolume)
		{
			case 0:
				sceneService.LoadScene(selectionStage.Scene.SceneName);
				break;
			case 1:
				sceneService.LoadScene(levelSelect[selectedVolume].Scene.SceneName);
				break;
			case 2:
				sceneService.LoadScene(levelSelect[selectedVolume].Scene.SceneName);
				break;
			default:
				sceneService.LoadScene(selectionStage.Scene.SceneName);
				break;
		}

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
			StartSelection(selectionType, SelectionUsesCanvas, SelectionRequiresVolume);
		else
			StopSelection();
	}

	[Button]
	public void SetRequiresVolume()
	{
		PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.SELECTION_REQUIRES_VOLUME, !SelectionRequiresVolume);
	}

	private void StartSelection(SelectionType type, bool useCanvas, bool requiresVolume)
	{
		canvasObject.SetActive(false);
		if (PhotonNetwork.IsMasterClient)
		{
			PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.SELECTION_TYPE, (int)type);
			PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.SELECTION_IS_ACTIVE, true);
			PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.SELECTION_USES_CANVAS, useCanvas);
			PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.SELECTION_REQUIRES_VOLUME, requiresVolume);
		}
	}

	public void StopSelection()
	{
		canvasObject.SetActive(false);
		if (PhotonNetwork.IsMasterClient)
		{
			PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.SELECTION_IS_ACTIVE, false);
			PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.SELECTION_TYPE, 0);
			PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.SELECTION_USES_CANVAS, false);
			PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.SELECTION_REQUIRES_VOLUME, true);
		}
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

	private void OnConnectionSuccessEvent(ConnectionSuccessEvent obj)
	{
		OnRoomPropertiesChangedEvent(PhotonNetwork.CurrentRoom.CustomProperties);
	}

	private void OnSceneLoadedEvent(SceneLoadedEvent obj)
	{
		timeGestureHeld = 0;
		if (PhotonNetwork.CurrentRoom != null)
			OnRoomPropertiesChangedEvent(PhotonNetwork.CurrentRoom.CustomProperties);
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
			{
				playerIsReady = (bool)r;
				if (!SelectionRequiresVolume)
					choices.Add((playerIsReady) ? 1 : 0);
			}

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
			text += "Popular choice is: " + GetTextForVolume(PopularChoice);
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
				photonView.RPC(nameof(SelectionEventRPC), RpcTarget.MasterClient);
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
			photonView.RPC(nameof(SelectionEventRPC), RpcTarget.MasterClient);
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

	public bool IsVolumeActive(int volumeID)
	{
		if (selectionVolumeTexts.ContainsKey(selectionType))
		{
			return volumeID <= selectionVolumeTexts[selectionType].Length - 1;
		}

		return false;
	}

	public string GetTextForVolume(int volumeID)
	{
		string text = "Error getting text";

		if(selectionVolumeTexts.ContainsKey(selectionType))
		{
			string[] texts = selectionVolumeTexts[selectionType];
			if (volumeID <= texts.Length - 1)
				text = texts[volumeID];
		}

		return text;
	}

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
