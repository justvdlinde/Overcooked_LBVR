using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Ingredient))]
public class CookComponent : MonoBehaviourPun
{
    public Ingredient ingredient;
    public CookStatus status = CookStatus.Raw;

    public bool isCooking;
    public float cookAmount = 0;
    public float rawToCookTime = 1f;
    public float cookedToBurnTime = 1f;
    public float burnedToFireTime = 1f;

    public bool canLightAflame = false;

    public AudioPlayerScript audioScript;

    public AudioClip cooking;
    public AudioClip isCooked;
    public AudioClip isBurned;

    [SerializeField] private GameObject rawPrefab = null;
    [SerializeField] private GameObject cookedPrefab = null;
    [SerializeField] private GameObject burntPrefab = null;

    [SerializeField] private ParticleSystem cookingParticles = null;
    [SerializeField] private ParticleSystem cookedToBurntParticles = null;
    [SerializeField] private ParticleSystem burntParticles = null;
    [SerializeField] private ParticleSystem fireParticles = null;

	[SerializeField] private GameObject progressBarObject = null;
	[SerializeField] private Image progressGraphics = null;
    [SerializeField] private Image progressGraphicsSphere = null;

	[SerializeField] private Color RawColor = new Color();
	[SerializeField] private Color CookedColor = new Color();
	[SerializeField] private Color BurntColor = new Color();
	[SerializeField] private Animator warningAnimator = null;
	[SerializeField] private GameObject warningObject = null;


	private float yOffset = 0f;
	private float yOffsetWarning = 0f;

	private void Awake()
	{
        audioScript = GetComponent<AudioPlayerScript>();
        SetAssetState();

		yOffset = progressBarObject.transform.localPosition.y;
		yOffsetWarning = warningObject.transform.localPosition.y;

        if (cookingParticles.isPlaying)
            cookingParticles.Stop();
        if (cookedToBurntParticles.isPlaying)
            cookedToBurntParticles.Stop();
        if (burntParticles.isPlaying)
            burntParticles.Stop();
    }

	private bool ShouldWarn()
	{
		return cookAmount >= rawToCookTime + (cookedToBurnTime * 0.5f) && status != CookStatus.Burned;
	}

	private void Update()
	{
		progressBarObject.SetActive(isCooking && status != CookStatus.Burned);
		warningObject.SetActive(isCooking && ShouldWarn());

		if (isCooking)
		{
			progressBarObject.transform.position = transform.position + Vector3.up * yOffset;
			warningObject.transform.position = transform.position + Vector3.up * yOffsetWarning;

			warningAnimator.SetBool("ShouldAnimate", ShouldWarn());

			progressBarObject.transform.LookAt(Camera.main.transform);
			warningObject.transform.LookAt(Camera.main.transform);
			DoParticles();
			Vector3 scale = new Vector3(Mathf.Clamp01(GetBarProgressScaled()), 1f, 1f);
			progressGraphics.transform.localScale = scale;
			progressGraphics.color = GetBarColor();
            progressGraphicsSphere.color = GetBarColor();


        }
		else
		{
			if (cookingParticles.isPlaying)
				cookingParticles.Stop();
			if (cookedToBurntParticles.isPlaying)
				cookedToBurntParticles.Stop();
			if (burntParticles.isPlaying)
				burntParticles.Stop();
		}
	}

	public float GetBarProgress()
	{
		float progress = cookAmount;
		if(cookAmount > rawToCookTime)
		{
			progress -= rawToCookTime;
			return Mathf.InverseLerp(0, cookedToBurnTime, progress);
		}
		else
		{
			return Mathf.InverseLerp(0, rawToCookTime, progress);
		}
	}

    public float GetBarProgressScaled()
	{
        float rawToCookedScale = Mathf.Clamp01(Mathf.InverseLerp(0, rawToCookTime, cookAmount));
        float cookToBurnScale = Mathf.Clamp01(Mathf.InverseLerp(0, cookedToBurnTime, cookAmount - rawToCookTime));

        return Mathf.Lerp(0.0f, 0.6f, rawToCookedScale) + Mathf.Lerp(0.0f, 0.3f, cookToBurnScale);
	}

	private Color GetBarColor()
	{
		float progress = GetBarProgress();
		if (cookAmount > rawToCookTime)
			return Color.Lerp(CookedColor, BurntColor, progress);
		else
			return Color.Lerp(RawColor, CookedColor, progress);
	}

    private void DoParticles()
	{
        if (!isCooking)
		{
            if (cookingParticles.isPlaying)
                cookingParticles.Stop();
            if (cookedToBurntParticles.isPlaying)
                cookedToBurntParticles.Stop();
            if (burntParticles.isPlaying)
                burntParticles.Stop();
            return;
        }

        if(cookAmount < rawToCookTime)
		{
            if (!cookingParticles.isPlaying)
                cookingParticles.Play();
            if (cookedToBurntParticles.isPlaying)
                cookedToBurntParticles.Stop();
            if (burntParticles.isPlaying)
                burntParticles.Stop();
        }
        else if (cookAmount >= rawToCookTime && cookAmount < cookedToBurnTime)
        {
            if (cookingParticles.isPlaying)
                cookingParticles.Stop();
            if (!cookedToBurntParticles.isPlaying)
                cookedToBurntParticles.Play();
            if (burntParticles.isPlaying)
                burntParticles.Stop();
        }
        else if (cookAmount >= cookedToBurnTime)
        {
            if (cookingParticles.isPlaying)
                cookingParticles.Stop();
            if (cookedToBurntParticles.isPlaying)
                cookedToBurntParticles.Stop();
            if (!burntParticles.isPlaying)
                burntParticles.Play();
        }

    }

	private void OnValidate()
    {
        if (ingredient == null)
            ingredient = GetComponent<Ingredient>();
        switch (status)
        {
            case CookStatus.Raw:

                cookAmount = 0;
                break;
            case CookStatus.Cooked:
                cookAmount = rawToCookTime;
                
                break;
            case CookStatus.Burned:
                cookAmount = rawToCookTime + cookedToBurnTime;
                
                break;
        }
    }

    private void SetAssetState()
	{
        rawPrefab?.SetActive(status == CookStatus.Raw);
        cookedPrefab?.SetActive(status == CookStatus.Cooked);
        burntPrefab?.SetActive(status == CookStatus.Burned);
    }

    public void Cook(float add = 1)
    {
        if (status != CookStatus.Burned)
        {
            if (status == CookStatus.Raw && cookAmount > rawToCookTime)
            {
                audioScript.PlayNonColSound(audioScript.cookSound, transform.position);
                SetState(CookStatus.Cooked);
                
            }
            else if (status == CookStatus.Cooked && cookAmount > rawToCookTime + cookedToBurnTime)
            {
                audioScript.PlayNonColSound(audioScript.burnSound, transform.position);
                SetState(CookStatus.Burned);
            }
        }
        SetAssetState();

        DoParticles();

        if (status != CookStatus.Raw)
            ingredient.SetState(IngredientStatus.Processed);

        cookAmount += add * Time.deltaTime;
    }

    public void SetState(CookStatus status)
    {
        photonView.RPC(nameof(SetCookStateRPC), RpcTarget.All, (int)status);
    }

    [PunRPC]
    private void SetCookStateRPC(int statusIndex, PhotonMessageInfo info)
    {
        status = (CookStatus)statusIndex;
    }

    public void playCookingSound()
    {
        audioScript.PlayNonColSound(audioScript.isCookingSound, transform.position);
    }
}