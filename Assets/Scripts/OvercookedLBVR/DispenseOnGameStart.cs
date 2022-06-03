using Photon.Pun;
using Photon.Realtime;
using System;
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
        globalEventDispatcher.Subscribe<ConnectionSuccessEvent>(OnConnected);
    }

    private void OnDisable()
    {
        globalEventDispatcher.Unsubscribe<StartGameEvent>(OnStartGameEvent);
        globalEventDispatcher.Unsubscribe<ConnectionSuccessEvent>(OnConnected);
    }

    private void OnConnected(ConnectionSuccessEvent obj)
    {
        if (PhotonNetwork.IsMasterClient)
            connectedDispenser.DispenseObject();
    }

    private void OnStartGameEvent(StartGameEvent obj)
    {
        if (photonView.IsMine)
            connectedDispenser.DispenseObject();
    }
}
