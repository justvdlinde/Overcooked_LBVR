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
public class SelectionManager : MonoBehaviour
{
    private SelectionPawn pawn = null;

	public static bool IsSelectionActive = true;

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

		globalEventDispatcher.Subscribe<TurnkeySelectionEvent>(OnSelectionEvent);

		if(pawn == null)
		{
			// some error show
		}
	}

	[Button]
	public void ToggleSelection()
	{
		IsSelectionActive = !IsSelectionActive;
	}

	private void OnSelectionEvent(TurnkeySelectionEvent obj)
	{
		currentPlayerEventState = obj;
		//Debug.Log($"isInVolume: {obj.isInVolume} volumeID: {obj.volumeID}");
	}

	private void OnDisable()
	{
		// left empty intentionally
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

		if (IsPawnReady)
			text += "1/1 players ready";
		else
			text += "0/1 players ready";

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
			isReadyBuzzRoutine = StartCoroutine(BuzzRoutine());
		IsPawnReady = true;
		// some event call
	}

	private void SetReadyToFalse()
	{
		if (isReadyBuzzRoutine != null && IsPawnReady)
			StopCoroutine(isReadyBuzzRoutine);
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
