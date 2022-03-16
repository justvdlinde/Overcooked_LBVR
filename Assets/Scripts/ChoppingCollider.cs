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

	public void PlaySound()
	{
		// left empty for now. Add in sound here
	}
}
