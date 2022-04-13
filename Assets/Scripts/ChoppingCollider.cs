using System.Collections.Generic;
using UnityEngine;

public class ChoppingCollider : MonoBehaviour
{
    public int HitDamage => hitDamage;

    [SerializeField] private int hitDamage = 1;
    [SerializeField] private List<Collider> connectedColliders = new List<Collider>();

    public void ToggleCollision(Collider collider, bool ignoreCollisions)
	{
		foreach (Collider connectedCollider in connectedColliders)
		{
			Physics.IgnoreCollision(collider, connectedCollider, ignoreCollisions);
		}
    }

    // TODO: replace with regular audiosource implementation
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
