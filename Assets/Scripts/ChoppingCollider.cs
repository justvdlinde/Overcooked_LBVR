using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoppingCollider : MonoBehaviour
{
    [SerializeField] private int hitDamage = 1;
    public int HitDamage => hitDamage;
    

   [SerializeField] private List<Collider> connectedColliders = new List<Collider>();

    public void ToggleCollision(Collider col, bool ignoreCollisions)
	{
		foreach (Collider c in connectedColliders)
		{
			Physics.IgnoreCollision(col, c, ignoreCollisions);
		}
        

    }

	public void PlaySound(AudioClip clip, Vector3 position)
	{

        GameObject obj = new GameObject();
        obj.transform.position = position;
        obj.AddComponent<AudioSource>();
        obj.GetComponent<AudioSource>().pitch = Random.Range(0.8f, 1.2f);
        obj.GetComponent<AudioSource>().PlayOneShot(clip);
        Destroy(obj, clip.length);
        return;



    }
}
