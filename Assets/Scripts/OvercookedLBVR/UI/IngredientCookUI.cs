using UnityEngine;
using UnityEngine.UI;

public class IngredientCookUI : MonoBehaviour
{
	private const string ANIMATOR_WARNING = "ShouldAnimate";

	[SerializeField] private IngredientCookController cookable = null;
	[SerializeField] private GameObject root = null;

	[Header("Progress Bar")]
	[SerializeField] private Slider progressBar = null;
	[SerializeField] private Image progressBarImage = null;
	[SerializeField] private Image progressGraphicsSphere = null;
	
	[Header("Colors")]
	[SerializeField] private Color RawColor = new Color();
	[SerializeField] private Color CookedColor = new Color();
	[SerializeField] private Color BurntColor = new Color();

	[Header("Warning Image")]
	[SerializeField] private Animator warningAnimator = null;
	[SerializeField] private GameObject warningObject = null;

	// TODO: clean up/merge these 2 functions?
	public float GetBarProgress()
	{
		float progress = cookable.CookProgress;
		if (cookable.CookProgress > cookable.RawToCookTime)
		{
			progress -= cookable.RawToCookTime;
			return Mathf.InverseLerp(0, cookable.CookedToBurnTime, progress);
		}
		else
		{
			return Mathf.InverseLerp(0, cookable.RawToCookTime, progress);
		}
	}

	public float GetCookProgressNormalized()
	{
		float rawToCookedScale = Mathf.Clamp01(Mathf.InverseLerp(0, cookable.RawToCookTime, cookable.CookProgress));
		float cookToBurnScale = Mathf.Clamp01(Mathf.InverseLerp(0, cookable.CookedToBurnTime, cookable.CookProgress - cookable.RawToCookTime));

		// TODO: clean up magic numbers
		return Mathf.Lerp(0.0f, 0.6f, rawToCookedScale) + Mathf.Lerp(0.0f, 0.3f, cookToBurnScale);
	}

	private Color GetBarColor()
	{
		float progress = GetBarProgress();
		if (cookable.CookProgress > cookable.RawToCookTime)
			return Color.Lerp(CookedColor, BurntColor, progress);
		else
			return Color.Lerp(RawColor, CookedColor, progress);
	}

	// TODO: replace with cook start/stop event + sync on startup?
	private void Update()
	{
		root.SetActive(cookable.IsCooking && cookable.State != CookState.Burned && !cookable.IsUnderCookingHood);
		warningObject.SetActive(cookable.IsCooking && ShouldWarningShow() && !cookable.IsUnderCookingHood);

		if (cookable.IsCooking)
		{
			warningAnimator.SetBool(ANIMATOR_WARNING, ShouldWarningShow());
			progressBar.value = GetCookProgressNormalized();
			SetProgressBarColors(GetBarColor());
		}
	}

	private void SetProgressBarColors(Color color)
    {
		progressBarImage.color = color;
		progressGraphicsSphere.color = color;
	}

	private bool ShouldWarningShow()
	{
		// TODO: clean up magic number
		return cookable.CookProgress >= cookable.RawToCookTime + (cookable.CookedToBurnTime * 0.5f) && cookable.State != CookState.Burned;
	}
}
