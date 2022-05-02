using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Utils.Core.Attributes;
using Utils.Core.Extensions;

public class IngredientCookController : MonoBehaviourPunCallbacks
{
    public bool IsCookable => ingredient.State == IngredientStatus.Processed;

    public CookState State => state;
    public bool IsCooking { get; private set; }
    public float CookProgress { get; private set; }

    public float RawToCookTime => rawToCookTime;
    public float CookedToBurnTime => cookedToBurnTime;
    public float BurnedToFireTime => burnedToFireTime;

    [SerializeField] private Ingredient ingredient = null;
    [SerializeField] private CookState state = CookState.Raw;

    [SerializeField] private float rawToCookTime = 1f;
    [SerializeField] private float cookedToBurnTime = 1f;
    [SerializeField] private float burnedToFireTime = 1f;

    [Header("Graphics")]
    [SerializeField] private GameObject rawGraphic = null;
    [SerializeField] private GameObject cookedGraphic = null;
    [SerializeField] private GameObject burntGraphic = null;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource = null;
    [SerializeField] private AudioClip[] cookingAudioClips = null;
    [SerializeField, Range(0, 1)] private float pitchDeviation = 0.2f;

    [Header("Particles")]
    [SerializeField] private ParticleSystem cookingParticles = null;
    [SerializeField] private ParticleSystem cookedToBurntParticles = null;
    [SerializeField] private ParticleSystem burntParticles = null;

    private void OnValidate()
    {
        if (ingredient == null)
            ingredient = transform.GetComponentInParent<Ingredient>();
    }

    private void Awake()
	{
        UpdateGraphicsToState(state);

        if (cookingParticles.isPlaying)
            cookingParticles.Stop();
        if (cookedToBurntParticles.isPlaying)
            cookedToBurntParticles.Stop();
        if (burntParticles.isPlaying)
            burntParticles.Stop();
    }

    public override void OnPlayerEnteredRoom(PhotonNetworkedPlayer newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
            SendSyncData(newPlayer);
    }

    private void SendSyncData(PhotonNetworkedPlayer player)
    {
        photonView.RPC(nameof(SendSyncDataRPC), player, (int)state, CookProgress);
    }

    [PunRPC]
    private void SendSyncDataRPC(object data, float cookProgress)
    {
        SetState((CookState)data);
        CookProgress = cookProgress;
    }

    private void Update()
	{
		if (IsCooking)
		{
			PlayParticles();
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

        transform.rotation = Quaternion.identity;
    }

    public void SetCookStatus(bool cook)
    {
        IsCooking = cook;
        if (cook)
            PlayCookingSound();
    }

    private void UpdateGraphicsToState(CookState state)
    {
        if(rawGraphic != null)
            rawGraphic.SetActive(state == CookState.Raw);
        if(cookedGraphic != null)
            cookedGraphic.SetActive(state == CookState.Cooked);
        if(burntGraphic != null)
            burntGraphic.SetActive(state == CookState.Burned);
    }

    public void Cook(float add = 1)
    {
        if (state != CookState.Burned)
        {
            if (state == CookState.Raw && CookProgress > rawToCookTime)
            {
                // replace with audiosource
                //audioScript.PlayNonColSound(audioScript.cookSound, transform.position);
                SetState(CookState.Cooked);
            }
            else if (state == CookState.Cooked && CookProgress > rawToCookTime + cookedToBurnTime)
            {
                // replace with audiosource
                //audioScript.PlayNonColSound(audioScript.burnSound, transform.position);
                SetState(CookState.Burned);
            }
        }

        PlayParticles();
        CookProgress += add * Time.deltaTime;
    }

    public void SetState(CookState status)
    {
        photonView.RPC(nameof(SetCookStateRPC), RpcTarget.All, (int)status);
    }

    [PunRPC]
    private void SetCookStateRPC(int statusIndex)
    {
        state = (CookState)statusIndex;
        UpdateGraphicsToState(state);

        switch (state)
        {
            case CookState.Raw:
                CookProgress = 0;
                break;
            case CookState.Cooked:
                CookProgress = rawToCookTime;
                break;
            case CookState.Burned:
                CookProgress = rawToCookTime + cookedToBurnTime;
                break;
        }
    }

    private void PlayCookingSound()
    {
        audioSource.pitch = Random.Range(1 - pitchDeviation, 1 + pitchDeviation);
        audioSource.PlayOneShot(cookingAudioClips.GetRandom());
    }

    // TODO: clean up (seperate particles class?)
    private void PlayParticles()
    {
        if (!IsCooking)
        {
            if (cookingParticles.isPlaying)
                cookingParticles.Stop();
            if (cookedToBurntParticles.isPlaying)
                cookedToBurntParticles.Stop();
            if (burntParticles.isPlaying)
                burntParticles.Stop();
            return;
        }

        if (CookProgress < rawToCookTime)
        {
            if (!cookingParticles.isPlaying)
                cookingParticles.Play();
            if (cookedToBurntParticles.isPlaying)
                cookedToBurntParticles.Stop();
            if (burntParticles.isPlaying)
                burntParticles.Stop();
        }
        else if (CookProgress >= rawToCookTime && CookProgress < cookedToBurnTime)
        {
            if (cookingParticles.isPlaying)
                cookingParticles.Stop();
            if (!cookedToBurntParticles.isPlaying)
                cookedToBurntParticles.Play();
            if (burntParticles.isPlaying)
                burntParticles.Stop();
        }
        else if (CookProgress >= cookedToBurnTime)
        {
            if (cookingParticles.isPlaying)
                cookingParticles.Stop();
            if (cookedToBurntParticles.isPlaying)
                cookedToBurntParticles.Stop();
            if (!burntParticles.isPlaying)
                burntParticles.Play();
        }
    }

#region Debug
    [Button]
    private void SetStateRawDebug()
    {
        SetState(CookState.Raw);
    }

    [Button]
    private void SetStateCookedDebug()
    {
        SetState(CookState.Cooked);
    }

    [Button]
    private void SetStateBurntDebug()
    {
        SetState(CookState.Burned);
    }
#endregion
}