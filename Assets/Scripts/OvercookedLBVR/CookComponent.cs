using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Ingredient))]
public class CookComponent : MonoBehaviourPun
{
    public Ingredient ingredient;
    public CookStatus status = CookStatus.Raw;

    public bool isCooking;
    public float cookAmount = 0;
    public float rawToCookTime = 1f;
    public float cookedToBurnTime = 1f;

    public AudioPlayerScript audioScript;

    public AudioClip cooking;
    public AudioClip isCooked;
    public AudioClip isBurned;

    [SerializeField] private GameObject rawPrefab   = null;
    [SerializeField] private GameObject cookedPrefab = null;
    [SerializeField] private GameObject burntPrefab     = null;

    [SerializeField] private ParticleSystem cookingParticles = null;
    [SerializeField] private ParticleSystem cookedToBurntParticles = null;
    [SerializeField] private ParticleSystem burntParticles = null;

	private void Awake()
	{
        audioScript = GetComponent<AudioPlayerScript>();
        SetAssetState();

        if (cookingParticles.isPlaying)
            cookingParticles.Stop();
        if (cookedToBurntParticles.isPlaying)
            cookedToBurntParticles.Stop();
        if (burntParticles.isPlaying)
            burntParticles.Stop();
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