using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCollisionTest : MonoBehaviour
{
	[SerializeField] private ParticleSystem ps = null;

	GameObject go1 = null;
	List<ParticleCollisionEvent> cEvents = new List<ParticleCollisionEvent>();
	private void OnParticleCollision(GameObject other)
	{
		int numCols = ps.GetCollisionEvents(other, cEvents);

		if (numCols > 0)
			go1.transform.position = cEvents[0].intersection;
	}
}
