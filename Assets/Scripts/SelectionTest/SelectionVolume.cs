using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionVolume : MonoBehaviour
{
    private Trigger trigger = null;

	private void OnEnable()
	{
		trigger.EnterEvent += OnEnterEvent;
		trigger.ExitEvent += OnExitEvent;
	}

	private void OnDisable()
	{
		trigger.EnterEvent -= OnEnterEvent;
		trigger.ExitEvent -= OnExitEvent;
	}

	private void OnExitEvent(Collider obj)
	{
		throw new NotImplementedException();
	}

	private void OnEnterEvent(Collider obj)
	{
		throw new NotImplementedException();
	}
}
