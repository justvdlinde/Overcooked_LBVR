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
            float maxAudio = (heatSource.IsHoodClosed) ? maxAudioVolume : maxAudioVolume * 0.5f;
            audioSource.volume = Mathf.Lerp(audioSource.volume, maxAudio, audioVolumeLerpSpeed * Time.deltaTime);
        }

        if (heatSource.CookedItems.Count < 1)
        {
            if (!audioSource.isPlaying)
                audioSource.Stop();
            audioSource.volume = Mathf.Lerp(audioSource.volume, 0, audioVolumeLerpSpeed * Time.deltaTime);
        }
    }
}
