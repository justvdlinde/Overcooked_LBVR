using Photon.Pun;
using System;
using System.Collections;
using Utils.Core.Events;
using Utils.Core.SceneManagement;
using Utils.Core.Services;

/// <summary>
/// Extension of <see cref="SceneService"/> that updates the Room scene properties and other networked scene related functions
/// </summary>
public class NetworkedSceneService : SceneService
{
    public override float LoadingProgress => PhotonNetwork.LevelLoadingProgress;

	public NetworkedSceneService(CoroutineService coroutineService, GlobalEventDispatcher globalEventDispatcher) : base(coroutineService, globalEventDispatcher) { }

    public override void LoadScene(string sceneName, Action<string> onDone = null)
    {
		UnityEngine.Debug.LogWarning("Loading scene is not synced for now! Call LoadSceneAsync.");
        base.LoadScene(sceneName, onDone);
    }

    public override IEnumerator LoadSceneAsyncCoroutine(string scene, Action<string> onDone = null)
    {
		PhotonNetwork.LoadLevel(scene);

		if (PhotonNetwork.IsMasterClient)
			PhotonNetwork.CurrentRoom.SetCustomProperty(RoomPropertiesPhoton.SCENE, scene);

		while (PhotonNetwork.AsyncLevelLoadingOperation != null && !PhotonNetwork.AsyncLevelLoadingOperation.isDone)
		{
			SceneLoadOperation = PhotonNetwork.AsyncLevelLoadingOperation;
			yield return null;
		}

		onDone?.Invoke(scene);
    }
}

