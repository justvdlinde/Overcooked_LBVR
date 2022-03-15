using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsCharacter
{
	public class PhysicsFingerController : MonoBehaviour
	{
		[SerializeField] private Trigger fingertipTrigger = null;
		[SerializeField] private FingerController fingerController = null;

		private List<Collider> registeredColliders = null;

		// gets set to lower upon freezing
		public float fingerUpperRange { get; private set; } = 1.0f;

		public bool canFreeze = true;
		public bool isFrozen { get; private set; } = false;

		private void Start()
		{
			if(fingertipTrigger == null)
				return;

			registeredColliders = new List<Collider>();

			fingertipTrigger.EnterEvent += OnCollisionEnterEvent;
			//fingertipTrigger.stayEvent += OnCollisionStayEvent;
			fingertipTrigger.ExitEvent += OnCollisionExitEvent;
		}

		public float GetFingerValue()
		{
			return fingerController.currentVal;
		}

		public float GetFingerValueRaw()
		{
			return fingerController.currentInputValue;
		}

		private void OnCollisionEnterEvent(Collider col)
		{
			if(!canFreeze || col.tag == "Player")
				return;
			// register current fold of finger to finger upper range
			// add to list
			if(!registeredColliders.Contains(col))
				registeredColliders.Add(col);

			if(registeredColliders.Count > 0)
			{
				fingerUpperRange = fingerController.currentVal;
				isFrozen = true;
			}
		}

		private void OnCollisionExitEvent(Collider col)
		{
			if(!canFreeze)
				return;
			// reset upper range to 1.0f
			// remove from list. reset if last in list/empty list

			if(registeredColliders.Contains(col))
				registeredColliders.Remove(col);

			if(registeredColliders.Count == 0)
			{
				UnfreezeFingers();
			}
		}

		private void UnfreezeFingers()
		{
			fingerUpperRange = 1.0f;
			isFrozen = false;
		}

		public void ForceUnFreezeFingers()
		{
			UnfreezeFingers();
			if(registeredColliders != null)
				registeredColliders.Clear();
		}
	}
}