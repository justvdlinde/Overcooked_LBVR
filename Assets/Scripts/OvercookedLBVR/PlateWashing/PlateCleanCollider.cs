using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateCleanCollider : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent(out PlateDirtySpot recipient))
		{
			Rigidbody rb = recipient.GetComponentInParent<Rigidbody>();

			recipient.DoCleanSpotSponge();
		}
	}
}
