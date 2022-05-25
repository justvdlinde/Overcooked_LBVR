using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Attributes;

public class PlateWashing : MonoBehaviour, IRottable
{
    public enum PlateState
    {
        Dirty,
        Clean
    }

    private PlateState currentPlateState = PlateState.Dirty;
	public PlateState CurrentPlateState { get => currentPlateState; set { currentPlateState = value; if (value == PlateState.Clean) IsPlateClean = true; else IsPlateClean = false; }  }


	public GameObject cleanGraphics = null;
    public GameObject dirtyGraphics = null;

    [SerializeField] private List<PlateDirtySpot> dirtySpots = null;

    [SerializeField] private bool isPlateClean = true;
	public bool IsPlateClean { get => isPlateClean; set { isPlateClean = value; if (value == true) SetPlateClean(); else SetPlateDirty(); } }
    [SerializeField] private ParticleSystem isCleanedParticles;

	public List<GameObject> toggleComponentsEnableOnClean = null;

    [SerializeField] private FoodStack foodStack = null;


	private Coroutine CountdownCoroutine = null;
	[SerializeField] private GameObject countdownCanvas = null;
    [SerializeField] private TMPro.TextMeshProUGUI countdownText = null;
    [SerializeField] private Color startColor = Color.white;
    [SerializeField] private Color endColor = Color.green;
    [SerializeField] private float yOffset = 0.0f;

	private void Awake()
	{
		yOffset = countdownCanvas.transform.localPosition.y;

		countdownCanvas.SetActive(false);
	}

	private void Start()
	{
        if (IsPlateClean)
            SetPlateClean();
        else
            SetPlateDirty();
	}

	private void Update()
	{
        if (IsPlateClean)
            return;
        int count = 0;
		foreach (var item in dirtySpots)
		{
            if (item.IsCleaned)
                count++;
            else
                break;
		}

        if(count >= dirtySpots.Count)
		{
            SetPlateClean();
		}

		countdownCanvas.transform.position = transform.position + Vector3.up * yOffset;
		countdownCanvas.transform.rotation = Quaternion.identity;
	}

    [Button]
    public void SetPlateDirty()
	{
        isPlateClean = false;
        currentPlateState = PlateState.Dirty;
        cleanGraphics.gameObject.SetActive(isPlateClean);
        dirtyGraphics.gameObject.SetActive(!isPlateClean);

        foodStack.RemoveAllIngredients();

        foreach (var item in toggleComponentsEnableOnClean)
		{
			item.SetActive(false);
		}
		// unstack all ingredients from the top
		// disable other scripts other than washable and physics
	}

    [Button]
    public void SetPlateClean()
	{
        currentPlateState = PlateState.Clean;
        if(isPlateClean == false)
            isCleanedParticles.Play();
        isPlateClean = true;
        cleanGraphics.gameObject.SetActive(isPlateClean);
        dirtyGraphics.gameObject.SetActive(!isPlateClean);

		foreach (var item in toggleComponentsEnableOnClean)
		{
			item.SetActive(true);
		}
        // enable all plate functionality
        // flip plate asset
    }

	public void SetIsRotting(bool isRotting)
	{
		if (currentPlateState == PlateState.Dirty)
			return;

		if (isRotting && !isFloorRotting)
		{
			StopAllCoroutines();
			CountdownCoroutine = StartCoroutine(StartCountdown(5));
		}
		else if (!isRotting)
		{
			StopCoroutine(CountdownCoroutine);
			countdownCanvas.SetActive(false);
			isFloorRotting = false;
		}
	}

	bool isFloorRotting = false;
	private IEnumerator StartCountdown(int seconds)
	{
		isFloorRotting = true;
		countdownCanvas.SetActive(true);
		countdownText.text = 5.ToString();
		countdownText.color = startColor;
		int count = 0;
		while (true)
		{
			yield return new WaitForSeconds(1);
			count++;
			countdownText.text = (5 - count).ToString();
			countdownText.color = Color.Lerp(startColor, endColor, (float)count / (float)seconds);

			if (count >= seconds)
				break;
		}
		countdownCanvas.SetActive(false);

		SetPlateDirty();
		isFloorRotting = false;
	}

}
