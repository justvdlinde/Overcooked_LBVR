using UnityEngine;

public class HeatSourceAudioController : MonoBehaviour
{
    [SerializeField] private HeatSource heatSource = null;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float maxAudioVolume;
    [SerializeField] private float audioVolumeLerpSpeed;

    private void Update()
    {
        // TODO: use events?
        if (heatSource.CookedItems.Count >= 1)
        {
            if (!audioSource.isPlaying)
                audioSource.Play();
            audioSource.volume = Mathf.Lerp(audioSource.volume, maxAudioVolume, audioVolumeLerpSpeed * Time.deltaTime);
        }

        if (heatSource.CookedItems.Count < 1)
        {
            if (!audioSource.isPlaying)
                audioSource.Stop();
            audioSource.volume = Mathf.Lerp(audioSource.volume, 0, audioVolumeLerpSpeed * Time.deltaTime);
        }
    }
}
