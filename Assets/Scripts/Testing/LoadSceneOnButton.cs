using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils.Core.SceneManagement;
using Utils.Core.Services;

public class LoadSceneOnButton : MonoBehaviour
{
    [SerializeField] protected GameLevel initialLevel = null;
	[SerializeField] private PhysicsButton button = null;

	private void Start()
	{
		button.PressEvent += LoadScene;
	}

	public void LoadScene()
	{
		if(PhotonNetwork.IsMasterClient)
		{
			GlobalServiceLocator.Instance.Get<GameModeService>().CurrentGameMode.EndGame();
			GlobalServiceLocator.Instance.Get<NetworkedSceneService>().LoadSceneAsync(initialLevel.Scene.SceneName);
		}
	}

}
