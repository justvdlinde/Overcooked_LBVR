using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Events;
using Utils.Core.Services;

public class DestroyOnGameStart : MonoBehaviour
{
    private GlobalEventDispatcher globalEventDispatcher;

    private void Awake()
    {
        globalEventDispatcher = GlobalServiceLocator.Instance.Get<GlobalEventDispatcher>();
    }

    private void OnEnable()
    {
        globalEventDispatcher.Subscribe<StartGameEvent>(OnGameStartEvent);
    }

    private void OnDisable()
    {
        globalEventDispatcher.Unsubscribe<StartGameEvent>(OnGameStartEvent);
    }

    private void OnGameStartEvent(StartGameEvent obj)
    {
        if (GetComponent<PhotonView>())
        {
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
