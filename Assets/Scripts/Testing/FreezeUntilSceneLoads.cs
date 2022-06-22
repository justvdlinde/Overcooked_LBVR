using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils.Core.Services;

public class FreezeUntilSceneLoads : MonoBehaviour
{
	private NetworkedSceneService networkedSceneService = null;

	[SerializeField] private Rigidbody connectedBody = null;

	private void Start()
	{
		networkedSceneService = GlobalServiceLocator.Instance.Get<NetworkedSceneService>();
		networkedSceneService.SceneLoadStartEvent += SceneLoadStartEvent;
		networkedSceneService.SceneLoadFinishEvent += SceneLoadFinishEvent;
	}

	bool gravAtload;
	bool kinAtload;

	private void SceneLoadStartEvent(string sceneName)
	{
		gravAtload = connectedBody.useGravity;
		kinAtload = connectedBody.isKinematic;
		connectedBody.isKinematic = true;
		connectedBody.useGravity = false;
	}

	private void SceneLoadFinishEvent(Scene scene, LoadSceneMode loadMode)
	{
		connectedBody.useGravity = gravAtload;
		connectedBody.isKinematic = kinAtload;
	}

	private void OnDestroy()
	{
		networkedSceneService.SceneLoadStartEvent -= SceneLoadStartEvent;
		networkedSceneService.SceneLoadFinishEvent -= SceneLoadFinishEvent;
	}
}
