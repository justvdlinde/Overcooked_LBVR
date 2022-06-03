using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Events;
using Utils.Core.Services;

public class DispenseOnGameStart : MonoBehaviourPun
{
    private GlobalEventDispatcher globalEventDispatcher = null;

    [SerializeField] private Dispenser connectedDispenser = null;

    private void OnEnable()
    {
        if (globalEventDispatcher == null)
            globalEventDispatcher = GlobalServiceLocator.Instance.Get<GlobalEventDispatcher>();

        globalEventDispatcher.Subscribe<StartGameEvent>(OnStartGameEvent);
    }

    private void OnDisable()
    {
        globalEventDispatcher.Unsubscribe<StartGameEvent>(OnStartGameEvent);
    }

	private void Start()
	{
		if(PhotonNetwork.IsMasterClient)
            connectedDispenser.DispenseObject();
    }

    private void OnStartGameEvent(StartGameEvent obj)
    {
        if (photonView.IsMine)
            connectedDispenser.DispenseObject();
    }
}
