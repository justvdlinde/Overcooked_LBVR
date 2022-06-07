using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils.Core.Services;

public class TimeLeftDisplay : MonoBehaviour
{
    [SerializeField] private Slider timer = null;

	[SerializeField] private Transform timerObject = null;
	[SerializeField] private TMPro.TextMeshProUGUI textDisplay = null;

	private GameModeService gameModeService = null;

	private string duringGameText = "Time untill close:";
	private string preGameText = "Welcome to McKnuckys!";

	private void Awake()
	{
		gameModeService = GlobalServiceLocator.Instance.Get<GameModeService>();
		timerObject.gameObject.SetActive(false);
		textDisplay.text = preGameText;
		timer.value = 1.0f;
	}

	private void Update()
	{
		if (gameModeService == null)
		{
			textDisplay.text = preGameText;
			timerObject.gameObject.SetActive(false);
			return;
		}

		if (gameModeService.CurrentGameMode == null)
		{
			textDisplay.text = preGameText;
			timerObject.gameObject.SetActive(false);
			return;
		}

		if (gameModeService.CurrentGameMode.MatchPhase == MatchPhase.Active)
		{
			timer.value = gameModeService.CurrentGameMode.GameTimeProgress01;

			textDisplay.text = duringGameText;
			timerObject.gameObject.SetActive(true);
		}
		else
		{
			//timer.value = gameModeService.CurrentGameMode.GameTimeProgress01;
			textDisplay.text = preGameText;
			timerObject.gameObject.SetActive(false);
		}
	}
}
