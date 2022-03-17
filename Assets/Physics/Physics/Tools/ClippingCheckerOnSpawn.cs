using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsCharacter
{
	public class ClippingCheckerOnSpawn : MonoBehaviour
	{
		[SerializeField] private Trigger enterTrigger = null;
		[SerializeField] private Trigger exitTrigger = null;
		private bool isActive = false;
		private bool isExitEntered = false;

		private int collidedCount = 0;

		private Hand currentGrippingHand = Hand.None;

		private bool canToggleCollider = true;

		private void Awake()
		{
			enterTrigger.EnterEvent += SetCollidersActive;

			exitTrigger.EnterEvent += SetIsExitEntered;

			exitTrigger.ExitEvent += SetCollidersInactive;

			enterTrigger.gameObject.SetActive(false);
			exitTrigger.gameObject.SetActive(false);
		}

		private void Update()
		{
			if(currentGrippingHand == Hand.None || !canToggleCollider)
				return;

			Hand h = (currentGrippingHand == Hand.Right) ? Hand.Left : Hand.Right;
			PhysicsPickupManager manager = (currentGrippingHand == Hand.Left) ? PhysicsPlayerBlackboard.Instance.rightHandPickupManager : PhysicsPlayerBlackboard.Instance.leftHandPickupManager;

			if(manager.IsHoldingObject())
				canToggleCollider = false;
		}

		public void EnableClippingChecker(Hand hand)
		{
			if(hand == Hand.None)
				return;
			currentGrippingHand = hand;

			Hand h = (currentGrippingHand == Hand.Right) ? Hand.Left : Hand.Right;
			PhysicsPlayerBlackboard.Instance.SetToolAndHandCollisions(h, true);

			enterTrigger.gameObject.SetActive(true);
			exitTrigger.gameObject.SetActive(true);
			canToggleCollider = true;
		}

		private void OnEnable()
		{
			if(currentGrippingHand == Hand.None)
				return;

			StartCoroutine(DisableTriggerAfterFrames(2));
		}

		private IEnumerator DisableTriggerAfterFrames(int amount)
		{
			for(int i = 0; i < amount; i++)
				yield return new WaitForEndOfFrame();

			enterTrigger.gameObject.SetActive(false);

			if(!isExitEntered)
			{
				exitTrigger.gameObject.SetActive(false);
				Hand h = (currentGrippingHand == Hand.Right) ? Hand.Left : Hand.Right;
				PhysicsPlayerBlackboard.Instance.SetToolAndHandCollisions(h, false);
			}
		}

		private void SetCollidersActive(Collider col)
		{
			int layerToCheck = (currentGrippingHand == Hand.Left) ? PhysicsPlayerBlackboard.RIGHT_HAND_LAYER_MASK : PhysicsPlayerBlackboard.LEFT_HAND_LAYER_MASK;

			if(col.tag == "Player" && col.gameObject.layer == layerToCheck)
			{
				Hand h = (currentGrippingHand == Hand.Right) ? Hand.Left : Hand.Right;
				PhysicsPlayerBlackboard.Instance.SetToolAndHandCollisions(h, true);
				isActive = true;
				collidedCount++;
			}
		}

		private void SetCollidersInactive(Collider col)
		{
			int layerToCheck = (currentGrippingHand == Hand.Left) ? PhysicsPlayerBlackboard.RIGHT_HAND_LAYER_MASK : PhysicsPlayerBlackboard.LEFT_HAND_LAYER_MASK;
			if(col.tag == "Player" && col.gameObject.layer == layerToCheck)
			{
				if(isExitEntered && currentGrippingHand != Hand.None)
				{
					collidedCount--;
					if(collidedCount <= 0)
					{
						collidedCount = 0;
						Hand h = (currentGrippingHand == Hand.Right) ? Hand.Left : Hand.Right;
						PhysicsPickupManager manager = (currentGrippingHand == Hand.Left) ? PhysicsPlayerBlackboard.Instance.rightHandPickupManager : PhysicsPlayerBlackboard.Instance.leftHandPickupManager;
						if(!manager.IsHoldingObject() && canToggleCollider)
							PhysicsPlayerBlackboard.Instance.SetToolAndHandCollisions(h, false);
					}
				}
				isActive = false;
			}
		}

		private void SetIsExitEntered(Collider col)
		{
			int layerToCheck = (currentGrippingHand == Hand.Left) ? PhysicsPlayerBlackboard.RIGHT_HAND_LAYER_MASK : PhysicsPlayerBlackboard.LEFT_HAND_LAYER_MASK;
			if(col.tag == "Player" && col.gameObject.layer == layerToCheck && !isExitEntered)
				isExitEntered = true;
		}

		public void ResetColliderObject()
		{
			StopAllCoroutines();
			isExitEntered = false;
			isActive = false;
			enterTrigger.gameObject.SetActive(false);
			exitTrigger.gameObject.SetActive(false);
			currentGrippingHand = Hand.None;
			collidedCount = 0;
		}
	}
}