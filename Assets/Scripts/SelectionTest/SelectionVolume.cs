using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Events;
using Utils.Core.Services;

public class SelectionVolume : MonoBehaviour
{
#if GAME_CLIENT
	[SerializeField] private Trigger trigger = null;
	
	public int selectionVolumeID = 0;

	private GlobalEventDispatcher globalEventDispatcher = null;

	public Color selectedColor = Color.green;
	public Color idleColor = Color.grey;

	private Renderer renderer = null;

	[SerializeField] private TMPro.TextMeshProUGUI text = null;

	private void Awake()
	{
		renderer = GetComponent<Renderer>();
		idleColor = renderer.material.color;
	}

	private void OnEnable()
	{
		if (globalEventDispatcher == null)
			globalEventDispatcher = GlobalServiceLocator.Instance.Get<GlobalEventDispatcher>();

		if (trigger == null)
			GetComponent<Trigger>();

		if(renderer == null)
			renderer = GetComponent<Renderer>();

		trigger.EnterEvent += OnEnterEvent;
		trigger.StayEvent += OnStayEvent;
		trigger.ExitEvent += OnExitEvent;
	}

	private void OnDisable()
	{
		trigger.EnterEvent -= OnEnterEvent;
		trigger.StayEvent -= OnStayEvent;
		trigger.ExitEvent -= OnExitEvent;
	}

	private void Update()
	{
		text.transform.forward = -(Camera.main.transform.position - text.transform.position);
	}

	private void OnEnterEvent(Collider obj)
	{
		if(obj.TryGetComponent<SelectionPawn>(out SelectionPawn pawn))
		{
			globalEventDispatcher.Invoke(new TurnkeySelectionEvent(selectionVolumeID, true));
			renderer.material.color = idleColor;
		}
	}

	private void OnStayEvent(Collider obj)
	{
		if (obj.TryGetComponent<SelectionPawn>(out SelectionPawn pawn))
		{
			if(pawn.IsPawnGesturingReady() && SelectionManager.IsPawnReady)
			{
				renderer.material.color = selectedColor;
			}
			else
			{
				renderer.material.color = idleColor;
			}
		}
	}

	private void OnExitEvent(Collider obj)
	{
		if (obj.TryGetComponent<SelectionPawn>(out SelectionPawn pawn))
		{
			globalEventDispatcher.Invoke(new TurnkeySelectionEvent(-1, false));
			renderer.material.color = idleColor;
		}
	}
#endif
}
