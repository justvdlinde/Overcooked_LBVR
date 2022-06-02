using PhysicsCharacter;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleObjectsOnPickup : MonoBehaviour
{
    [SerializeField] private Tool connectedTool = null;

	public List<GameObject> objectsToToggle = null;

	private void Awake()
	{
		connectedTool.OnLocalPickupEvent += OnPickupEvent;
		connectedTool.OnLocalDropEvent += OnDropEvent;
	}

	private void OnPickupEvent()
	{
		foreach (var item in objectsToToggle)
		{
			item.SetActive(!item.activeSelf);
		}
	}

	private void OnDropEvent()
	{
		foreach (var item in objectsToToggle)
		{
			item.SetActive(!item.activeSelf);
		}
	}
}
