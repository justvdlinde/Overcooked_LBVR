using UnityEngine;

[RequireComponent(typeof(Ingredient))]
public class CookComponent : MonoBehaviour
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



	private void Awake()
	{
        audioScript = GetComponent<AudioPlayerScript>();
        SetAssetState();
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
                audioScript.PlaySound(audioScript.cookSound, transform.position);
                status = CookStatus.Cooked;
                
            }
            else if (status == CookStatus.Cooked && cookAmount > rawToCookTime + cookedToBurnTime)
            {
                audioScript.PlaySound(audioScript.burnSound, transform.position);
                status = CookStatus.Burned;
                
            }
        }
        SetAssetState();

        if (status != CookStatus.Raw)
            ingredient.status = IngredientStatus.Processed;

        cookAmount += add * Time.deltaTime;
    }

    public void playCookingSound()
    {
        audioScript.PlaySound(audioScript.isCookingSound, transform.position);
    }

 

    }