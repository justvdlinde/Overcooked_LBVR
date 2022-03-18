using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayerScript : MonoBehaviour
{
    public AudioClip metalHit;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider col)
    {
        if(col.tag == "Metal")
        {
            GameObject obj = new GameObject();
            obj.transform.position = transform.position;
            obj.AddComponent<AudioSource>();
            obj.GetComponent<AudioSource>().pitch = UnityEngine.Random.Range(0.8f, 1.2f);
            obj.GetComponent<AudioSource>().PlayOneShot(metalHit);
            Destroy(obj, metalHit.length);
            return;
        }
    }
}
