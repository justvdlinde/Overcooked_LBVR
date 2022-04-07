using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaucePlacer : MonoBehaviour
{
	[SerializeField] private ParticleSystem ps = null;
	[SerializeField] private IngredientType sauceType = IngredientType.Ketchup;

	public bool IsActive = false;

	public void SetActive(bool enabled)
	{
		IsActive = enabled;
	}

	List<ParticleCollisionEvent> cEvents = new List<ParticleCollisionEvent>();
	private void OnParticleCollision(GameObject other)
	{
		int numCols = ps.GetCollisionEvents(other, cEvents);

		if (numCols > 0)
		{
			Collider[] cols = Physics.OverlapSphere(cEvents[0].intersection, 0.05f);
			foreach (Collider col in cols)
			{
				if(col.TryGetComponent<SauceRecipient>(out SauceRecipient recipient))
				{
					recipient.ProgressSauceValue(Time.deltaTime, sauceType);
				}
			}
		}
	}
}
