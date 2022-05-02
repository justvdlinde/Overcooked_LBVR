using UnityEngine;

public class FireExtinguisherController : InteractableTool
{
	[SerializeField] private new Rigidbody rigidbody = null;
    [SerializeField] private ParticleSystem[] particleSystems = null;
    [SerializeField] private AudioSource audioSourceLooping = null;
    [SerializeField] private AudioSource audioSourceOneshot = null;
    [SerializeField] private SauceCollisionDetector saucePlacer = null;

    private bool isActive = false;

	private void Update()
	{
		if(isActive)
		{
			rigidbody.AddForce(-rigidbody.transform.forward * 10f, ForceMode.Acceleration);
		}
	}

    protected override void OnUpEventInternal()
    {
        base.OnUpEventInternal();

        foreach (var particleSystem in particleSystems)
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

}
