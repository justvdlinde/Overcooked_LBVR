using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HeatSource : MonoBehaviour
{
    public float heatStrength = 1;
    public float volume;
    public float volumeSpeed;
    public AudioSource audioSouce;
    public List<GameObject> itemsInList = new List<GameObject>();



    private void Update()
    {
        if (itemsInList.Count >= 1)
        {
            audioSouce.volume = Mathf.Lerp(audioSouce.volume, volume, volumeSpeed);
        }

        if (itemsInList.Count < 1)
        {
            audioSouce.volume = Mathf.Lerp(audioSouce.volume, 0, volumeSpeed);
        }

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.TryGetComponent(out CookComponent cookable))
        {
            cookable.Cook(heatStrength);

        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out CookComponent cookable))
        {
            cookable.playCookingSound();
            cookable.isCooking = true;

            itemsInList.Add(other.gameObject);

        }
        



    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out CookComponent cookable))
        {
            if (cookable.isCooking)
                cookable.isCooking = false;
            itemsInList.Remove(other.gameObject);

        }

    }

  
}
