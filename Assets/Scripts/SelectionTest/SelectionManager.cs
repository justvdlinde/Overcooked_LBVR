using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

	public bool IsPawnReady = false;

	public float gesturingReadyHeightOffsetInMeters = 0.15f;
	public float timeToHoldGestureToRegisterReady = 5f;
	private float timeGestureHeld = 0.0f;

	private GlobalEventDispatcher globalEventDispatcher = null;

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

	private void OnSelectionEvent(TurnkeySelectionEvent obj)
	{
		throw new System.NotImplementedException();
	}

	private void OnDisable()
	{
		
	}

	private void Update()
	{
		if (pawn == null)
			return; // show some error

		if(IsSelectionActive)
		{
			Debug.Log($"pawn rdy {IsPawnReady} pawn gest rdy {pawn.IsPawnGesturingReady(gesturingReadyHeightOffsetInMeters)} left {pawn.IsPawnGesturingReady(gesturingReadyHeightOffsetInMeters, Hand.Left)} right {pawn.IsPawnGesturingReady(gesturingReadyHeightOffsetInMeters, Hand.Right)}");

			if(pawn.IsPawnGesturingReady(gesturingReadyHeightOffsetInMeters))
			{
				if (timeGestureHeld < timeToHoldGestureToRegisterReady)
				{
					timeGestureHeld += Time.deltaTime;

					if(pawn.IsPawnGesturingReady(gesturingReadyHeightOffsetInMeters, Hand.Left))
					{
						XRInput.PlayHaptics(Hand.Left, Mathf.Lerp(0.05f, 0.5f, Mathf.InverseLerp(0, timeToHoldGestureToRegisterReady, timeGestureHeld)), Time.deltaTime);
					}
					if (pawn.IsPawnGesturingReady(gesturingReadyHeightOffsetInMeters, Hand.Right))
					{
						XRInput.PlayHaptics(Hand.Right, Mathf.Lerp(0.05f, 0.5f, Mathf.InverseLerp(0, timeToHoldGestureToRegisterReady, timeGestureHeld)), Time.deltaTime);
					}
				}

				if(timeGestureHeld >= timeToHoldGestureToRegisterReady)
				{
					SetReadyToTrue();
				}
			}
			else
			{
				SetReadyToFalse();
			}
		}
	}

	private Coroutine isReadyBuzzRoutine = null;

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

	// translate this to some wave
	private IEnumerator BuzzRoutine()
	{
		yield return new WaitForEndOfFrame();

		if (pawn.IsPawnGesturingReady(gesturingReadyHeightOffsetInMeters, Hand.Left))
			XRInput.PlayHaptics(Hand.Left, 0.5f, 0.2f);
		if (pawn.IsPawnGesturingReady(gesturingReadyHeightOffsetInMeters, Hand.Right))
			XRInput.PlayHaptics(Hand.Right, 0.5f, 0.2f);

		yield return new WaitForSeconds(0.5f);

		if(pawn.IsPawnGesturingReady(gesturingReadyHeightOffsetInMeters, Hand.Left))
			XRInput.PlayHaptics(Hand.Left, 0.8f, 0.3f);
		if (pawn.IsPawnGesturingReady(gesturingReadyHeightOffsetInMeters, Hand.Right))
			XRInput.PlayHaptics(Hand.Right, 0.8f, 0.3f);

		yield return new WaitForSeconds(0.4f);

		if (pawn.IsPawnGesturingReady(gesturingReadyHeightOffsetInMeters, Hand.Left))
			XRInput.PlayHaptics(Hand.Left, 0.8f, 0.3f);
		if (pawn.IsPawnGesturingReady(gesturingReadyHeightOffsetInMeters, Hand.Right))
			XRInput.PlayHaptics(Hand.Right, 0.8f, 0.3f);
	}
}
