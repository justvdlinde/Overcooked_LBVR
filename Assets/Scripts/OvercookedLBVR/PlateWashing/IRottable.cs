public interface IRottable
{
	/// <summary>
	/// add these to implementing scripts
	/// 
	/// private Coroutine CountdownCoroutine = null;
	/// [SerializeField] private GameObject countdownCanvas = null;
	/// [SerializeField] private TMPro.TextMeshProUGUI countdownText = null;
	/// [SerializeField] private Color startColor = Color.white;
	/// [SerializeField] private Color endColor = Color.green;
	/// [SerializeField] private float yOffset = 0.0f;
	/// </summary>

	public void SetIsRotting(bool isRotting);

	// add these for the rotting components
	//public void SetIsRotting(bool isRotting)
	//{
	//	if (currentPlateState == PlateState.Dirty)
	//		return;

	//	if (isRotting && !isFloorRotting)
	//	{
	//		StopAllCoroutines();
	//		CountdownCoroutine = StartCoroutine(StartCountdown(5));
	//	}
	//	else if (!isRotting)
	//	{
	//		StopCoroutine(CountdownCoroutine);
	//		countdownCanvas.SetActive(false);
	//		isFloorRotting = false;
	//	}
	//}

	//bool isFloorRotting = false;
	//private IEnumerator StartCountdown(int seconds)
	//{
	//	isFloorRotting = true;
	//	countdownCanvas.SetActive(true);
	//	countdownText.text = 5.ToString();
	//	countdownText.color = startColor;
	//	int count = 0;
	//	while (true)
	//	{
	//		yield return new WaitForSeconds(1);
	//		count++;
	//		countdownText.text = (5 - count).ToString();
	//		countdownText.color = Color.Lerp(startColor, endColor, (float)count / (float)seconds);

	//		if (count >= seconds)
	//			break;
	//	}
	//	countdownCanvas.SetActive(false);

	//	SetPlateDirty();
	//	isFloorRotting = false;
	//}
}
