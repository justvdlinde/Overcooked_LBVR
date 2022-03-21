using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayerScript : MonoBehaviour
{

    public float maxVolume = 1f;

    public float volume = 1f;
    public float ownVelocity;
    public float incomingVelocity;

    //collision sounds
    [SerializeField] public AudioClip metalHit;
    [SerializeField] public AudioClip woodHit;
    [SerializeField] public AudioClip plateHit;

    [SerializeField] public AudioClip KnifeHit;
    [SerializeField] public AudioClip softFood;
    [SerializeField] public AudioClip crunchFood;

    [SerializeField] public AudioClip heatSource;

    //None collision sounds
    [SerializeField] public AudioClip isCookingSound;
    [SerializeField] public AudioClip cookSound;
    [SerializeField] public AudioClip burnSound;
    [SerializeField] public AudioClip doneSound;




    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider col)
    {

        Rigidbody[] rigidbodies = col.GetComponentsInParent<Rigidbody>();
        Rigidbody rigidbody = null;

        if (rigidbodies.Length <= 0) 
        { 
            rigidbodies = col.GetComponents<Rigidbody>();
        }
        foreach (Rigidbody rb in rigidbodies)
        {
            if (!rb.isKinematic)
            {
                rigidbody = rb;
                break;
            }
        }


        if (rigidbody == null)
            incomingVelocity = 0.1f;
        else
        {
            incomingVelocity = rigidbody.velocity.magnitude;

            Debug.Log(name + rigidbody.velocity.magnitude);
        }



            Debug.Log(transform.name + incomingVelocity);

        if(col.tag == Tags.METAL)
        {
            PlaySound(metalHit, transform.position);
            Debug.Log("play METAL");
        }

        if (col.tag == Tags.WOOD)
        {
            PlaySound(woodHit, transform.position);
            Debug.Log("play WOOD");
        }

        if (col.tag == Tags.PLATE)
        {
            PlaySound(plateHit, transform.position);
            Debug.Log("play PLATE");
        }

        if (col.tag == Tags.KNIFE)
        {
            PlaySound(KnifeHit, transform.position);
            Debug.Log("play KNIFA");
        }

        if (col.tag == Tags.FOOD_SOFT)
        {
            PlaySound(softFood, transform.position);
            Debug.Log("play SOFT");

        }

        if (col.tag == Tags.FOOD_CRUNCH)
        {
            PlaySound(crunchFood, transform.position);
            Debug.Log("play CRUNCH");
        }

        if (col.tag == Tags.HEAT_SOURCE)
        {
            PlaySound(heatSource, transform.position);
            Debug.Log("play HEAT");
        }

 
    }

    public void PlaySound(AudioClip clip, Vector3 position)
    {
        if(TryGetComponent(out Rigidbody rigidbody))
        {
            ownVelocity = rigidbody.velocity.magnitude;
        }
        else if (transform.parent.TryGetComponent(out Rigidbody rigidbodyB))
        {

            ownVelocity = rigidbodyB.velocity.magnitude;
        }
        else if (transform.parent.parent.TryGetComponent(out Rigidbody rigidbodyC))
        {

            ownVelocity = rigidbodyC.velocity.magnitude;
        }



        volume = (Mathf.InverseLerp(0, 2, incomingVelocity) + Mathf.InverseLerp(0, 2, ownVelocity)) * 0.5f;
        volume = Mathf.Lerp(0, maxVolume, volume);

        GameObject obj = new GameObject();
        obj.transform.position = position;
        obj.AddComponent<AudioSource>().volume = volume;
        obj.AddComponent<AudioSource>().spatialBlend = 1f;
        obj.GetComponent<AudioSource>().pitch = UnityEngine.Random.Range(0.8f, 1.2f);
        obj.GetComponent<AudioSource>().PlayOneShot(clip);
        Destroy(obj, metalHit.length);
        return;
    }
}
