using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoppingProcessable : MonoBehaviour
{
    [SerializeField] private Ingredient ingredient = null;
	[SerializeField] private Trigger trigger = null;

	private int currentHitsLeft = 0;
	[SerializeField] private int hitsNeededToProcess = 5;

	[SerializeField] private List<Collider> connectedColliders = new List<Collider>();

	private void Awake()
	{
		Init();
	}

	private void Init()
	{
		currentHitsLeft = hitsNeededToProcess;
		trigger.EnterEvent += OnEnterEvent;
		trigger.ExitEvent += OnExitEvent;
	}

	private void Disable()
	{
		trigger.EnterEvent -= OnEnterEvent;
		trigger.ExitEvent -= OnExitEvent;
	}

	private void OnEnterEvent(Collider obj)
	{
		if(obj.TryGetComponent<ChoppingCollider>(out ChoppingCollider col))
		{
			foreach (Collider c in connectedColliders)
			{
				col.ToggleCollision(c, true);
			}

			if (ingredient.status == IngredientStatus.UnProcessed && currentHitsLeft > 0)
			{
				currentHitsLeft -= col.HitDamage;
				Debug.Log($"Chopped {name} for {col.HitDamage} and has {currentHitsLeft} left");
			}
			else if (ingredient.status == IngredientStatus.UnProcessed && currentHitsLeft <= 0)
			{
				ingredient.Process();
				Disable();
			}
		}
	}

	private void OnExitEvent(Collider obj)
	{
		if (obj.TryGetComponent<ChoppingCollider>(out ChoppingCollider col))
		{
			foreach (Collider c in connectedColliders)
			{
				col.ToggleCollision(c, false);
			}

		}
	}

}
