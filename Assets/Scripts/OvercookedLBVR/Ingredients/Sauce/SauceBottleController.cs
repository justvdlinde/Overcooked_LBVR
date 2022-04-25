using UnityEngine;

public class SauceBottleController : InteractableTool
{
    public SauceType SauceType => sauceType;

    [SerializeField] private SauceType sauceType = SauceType.Ketchup;
    [SerializeField] private float sauceFillSpeed = 1;

    [Header("References")]
    [SerializeField] private ParticleSystem[] particleSystems = null;
    [SerializeField] private AudioSource audioSourceLooping = null;
    [SerializeField] private AudioSource audioSourceOneshot = null;
    [SerializeField] private SauceCollisionDetector saucePlacer = null;

    //public SauceRecipient testRecipient;

    //private void Update()
    //{
    //    if (Input.GetKey(KeyCode.Space))
    //        testRecipient.ApplySauce(sauceFillSpeed * Time.deltaTime, sauceType);
    //}

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
        recipient.ApplySauce(sauceFillSpeed * Time.deltaTime, SauceType);
    }
}
