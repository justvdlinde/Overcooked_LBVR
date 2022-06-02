using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireExtinguisherParticleSystem : MonoBehaviour
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
				IngredientStatusCondition statusCondition = other.GetComponentInChildren<IngredientStatusCondition>();
				if (statusCondition == null)
					statusCondition = other.GetComponentInParent<IngredientStatusCondition>();
				if (statusCondition != null)
				{
					statusCondition.AddHeat(-Time.deltaTime * 25f, StatusConditionHeatSource.Cold);
				}
			}
		}
	}
}
