using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Attributes;

public class PlateWashing : MonoBehaviour
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
	}

    [Button]
    public void SetPlateDirty()
	{
        isPlateClean = false;
        currentPlateState = PlateState.Dirty;
        cleanGraphics.gameObject.SetActive(isPlateClean);
        dirtyGraphics.gameObject.SetActive(!isPlateClean);

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
}
