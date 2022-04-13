using UnityEngine;

public class HeatSourceAudioController : MonoBehaviour
{
    [SerializeField] private HeatSource heatSource = null;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSouce;
    [SerializeField] private float maxAudioVolume;
    [SerializeField] private float audioVolumeLerpSpeed;

    private void Update()
    {
        if (heatSource.CookedItems.Count >= 1)
        {
            audioSouce.volume = Mathf.Lerp(audioSouce.volume, maxAudioVolume, audioVolumeLerpSpeed);
        }

        if (heatSource.CookedItems.Count < 1)
        {
            audioSouce.volume = Mathf.Lerp(audioSouce.volume, 0, audioVolumeLerpSpeed);
        }
    }
}
