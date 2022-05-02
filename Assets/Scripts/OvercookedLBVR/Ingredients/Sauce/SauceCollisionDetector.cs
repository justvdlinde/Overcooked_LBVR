using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Detects collision using particle system and calls <see cref="SauceRecipientDetected"/> 
/// </summary>
public class SauceCollisionDetector : MonoBehaviour
{
	private const float COLLISION_RADIUS = 0.05f;
	public Action<SauceRecipient> SauceRecipientDetected;

	[SerializeField] private new ParticleSystem particleSystem = null;
	private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

	private void OnParticleCollision(GameObject other)
	{
		int collisionCount = particleSystem.GetCollisionEvents(other, collisionEvents);

		if (collisionCount > 0)
		{
			Collider[] colliders = Physics.OverlapSphere(collisionEvents[0].intersection, COLLISION_RADIUS);
			foreach (Collider collider in colliders)
			{
				if(collider.TryGetComponent(out SauceRecipient recipient))
				{
					SauceRecipientDetected?.Invoke(recipient);
				}
			}
		}
	}
}
