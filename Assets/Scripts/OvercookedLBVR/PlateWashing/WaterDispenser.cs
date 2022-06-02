using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterDispenser : MonoBehaviour
{
	[SerializeField] private ParticleSystem particleSystem = null;
	private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
	private const float COLLISION_RADIUS = 0.05f;


	private void OnParticleCollision(GameObject other)
	{
		int collisionCount = particleSystem.GetCollisionEvents(other, collisionEvents);

		if (collisionCount > 0)
		{
			Collider[] colliders = Physics.OverlapSphere(collisionEvents[0].intersection, COLLISION_RADIUS);
			foreach (Collider collider in colliders)
			{
				if (collider.TryGetComponent(out PlateDirtySpot recipient))
				{
					recipient.DoCleanSpot();
				}
			}
		}
	}
}
