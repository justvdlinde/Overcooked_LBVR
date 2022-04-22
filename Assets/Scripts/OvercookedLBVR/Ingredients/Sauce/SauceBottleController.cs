using UnityEngine;

public class SauceBottleController : InteractableTool
{
    public IngredientType SauceType => sauceType;
    [SerializeField] private IngredientType sauceType = IngredientType.Ketchup;

    [Header("References")]
    [SerializeField] private ParticleSystem[] particleSystems = null;
    [SerializeField] private AudioSource audioSourceLooping = null;
    [SerializeField] private AudioSource audioSourceOneshot = null;
    [SerializeField] private SauceCollisionDetector saucePlacer = null;

    private void OnEnable()
    {
        saucePlacer.SauceRecipientDetected += OnSauceRecipientDetected;
    }

    private void OnDisable()
    {
        saucePlacer.SauceRecipientDetected -= OnSauceRecipientDetected;
    }

    protected override void OnUpEventInternal()
    {
        base.OnUpEventInternal();

        foreach(var particleSystem in particleSystems)
        {
            particleSystem.Stop();
        }

        audioSourceLooping.enabled = false;
    }

    protected override void OnDownEventInternal()
    {
        base.OnDownEventInternal();

        foreach (var particleSystem in particleSystems)
        {
            particleSystem.Play();
        }

        audioSourceLooping.enabled = true;
        audioSourceOneshot.Play();
    }

    private void OnSauceRecipientDetected(SauceRecipient recipient)
    {
        recipient.ProgressSauceValue(Time.deltaTime, SauceType);
    }
}
