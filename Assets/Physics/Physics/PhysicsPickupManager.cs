using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsCharacter
{
	public class PhysicsPickupManager : MonoBehaviour
	{
		[SerializeField] private Hand hand = Hand.None;
		public Hand Hand => hand;
		[SerializeField] private PhysicsPickupManager otherPickupManager = null;

		public PhysicsHandsController handPhysics = null;

		// make sure this list in is the order of: index, middle, ring, pinky, thumb (Fingers enum order)
		[SerializeField] private List<PhysicsFingerController> fingers = new List<PhysicsFingerController>();
		[SerializeField] private List<Collider> connectedColliders = new List<Collider>();

		[SerializeField] private Rigidbody rigidBody = null;
		[SerializeField] private PhysicMaterial nonGrippingPhysicsMaterial = null;
		[SerializeField] private PhysicMaterial grippingPhysicsMaterial = null;
		[SerializeField] private Trigger grippingTrigger = null;
		[SerializeField] private Transform gripPivot = null;
		[SerializeField] private Transform gripAnchor = null;

		[SerializeField] private Collider indexCol, middleCol, ringCol, pinkyCol, thumbCol;

		private const float MIN_GRIPPING_VALUE = 0.8f;

		private PickupableObject currentPickupable = null;

		private HandsDataDelegate handDelegate = null;

		[SerializeField] private LayerMask pickupablesMask = 0;

		private void Start()
		{
			handDelegate = HandsDataDelegate.GetHandedInstance(hand);

			// move gripAnchor to controller
			transform.position = handDelegate.GetFollowPosition();
			transform.rotation = handDelegate.GetFollowRotation();

			Vector3 localPos = gripAnchor.localPosition;
			Quaternion localRot = gripAnchor.localRotation;
			gripAnchor.transform.parent = handDelegate.GetFollowTarget();
		}

		public void ForceSetPickupable(PickupableObject obj)
		{
			if(obj == null)
				return;
			currentPickupable = obj;
		}

		public void ForceDropItem()
		{
			currentPickupable = null;
			foreach(PhysicsFingerController finger in fingers)
			{
				finger.ForceUnFreezeFingers();
			}
		}

		private void OnEnable()
		{
			//grippingTrigger.EnterEvent += OnGripEnter;
			//grippingTrigger.StayEvent += OnGripStay;
			//grippingTrigger.ExitEvent += OnGripExit;
		}

		private void OnDisable()
		{
			//grippingTrigger.EnterEvent -= OnGripEnter;
			//grippingTrigger.StayEvent -= OnGripStay;
			//grippingTrigger.ExitEvent -= OnGripExit;
		}

		private void Update()
		{
			bool handGripping = IsHandGripping();

			if(hand == Hand.Right)
				PhysicsPlayerBlackboard.Instance.isRightHandGripping = handGripping;
			else
				PhysicsPlayerBlackboard.Instance.isLeftHandGripping = handGripping;

			SetGrippingMaterial(handGripping);

			bool isIgnoredCollision = 
				(hand == Hand.Left) ? 
				Physics.GetIgnoreLayerCollision(PhysicsCharacter.PhysicsPlayerBlackboard.TOOL_LAYER_MASK, PhysicsCharacter.PhysicsPlayerBlackboard.LEFT_HAND_LAYER_MASK) : 
				Physics.GetIgnoreLayerCollision(PhysicsCharacter.PhysicsPlayerBlackboard.TOOL_LAYER_MASK, PhysicsCharacter.PhysicsPlayerBlackboard.RIGHT_HAND_LAYER_MASK);

			bool holdingObject = !isIgnoredCollision;
			indexCol.enabled = holdingObject;
			middleCol.enabled = holdingObject;
			ringCol.enabled = holdingObject;
			pinkyCol.enabled = holdingObject;
			thumbCol.enabled = holdingObject;

			if (currentPickupable == null)
			{
				Collider[] toolHandlesInRange = Physics.OverlapSphere(gripPivot.transform.position, 0.1f, pickupablesMask);
				List<PickupableObject> pickupablesInRange = new List<PickupableObject>();
				foreach (var item in toolHandlesInRange)
				{
					if (item.TryGetComponent<PickupableObject>(out PickupableObject p))
					{
						if (!pickupablesInRange.Contains(p))
							pickupablesInRange.Add(p);
					}
				}
				float closestDistance = 1000f;
				PickupableObject closestObject = null;
				foreach (var item in pickupablesInRange)
				{
					Vector3 closestPoint = item.objectCollider.ClosestPoint(gripPivot.transform.position);

					Debug.DrawLine(gripPivot.transform.position, closestPoint, Color.red, 0.0f);

					float dist = Vector3.Distance(gripPivot.transform.position, closestPoint);
					if (dist < closestDistance)
					{
						closestObject = item;
						closestDistance = dist;
					}
				}

				if(closestObject != null && IsHandGripping() && !isGrippingPrevious)
				{
					currentPickupable = closestObject;
					currentPickupable = currentPickupable.GetGrabbed(hand, gripAnchor);
				}
			}

			if (currentPickupable != null)
			{
				if (!IsHandGripping())
				{
					currentPickupable.GetReleased(hand);
					currentPickupable = null;
				}
				else if (!handDelegate.IsHandInRange())
				{
					currentPickupable.GetReleased(hand);
					currentPickupable = null;
				}
			}

			//if (currentPickupable != null && currentPickupable.IsObjectBeingHeld(hand) == false && !IsHandGripping() && Vector3.Distance(currentPickupable.transform.position, handDelegate.GetFollowPosition()) > 0.25f)
			//{
			//	ForceDropItem();
			//}

			isGrippingPrevious = handGripping;
		}

		public void OnGripEnter(Collider col)
		{
			if (col.TryGetComponent(out PickupableObject pickupable))
			{
				if (currentPickupable == null && !IsHandGripping())
					currentPickupable = pickupable;
			}
		}

		public void OnGripStay(Collider col)
		{
			if (col.TryGetComponent(out PickupableObject pickupable))
			{
				bool isGripping = IsHandGripping();
				if (pickupable == currentPickupable)
				{
					if (handDelegate.IsHandInRange() && isGripping && (isGrippingPrevious == false))
					{
						if (!currentPickupable.IsObjectBeingHeld(hand))
						{
							if (currentPickupable.IsObjectBeingHeld((hand == Hand.Right) ? Hand.Left : Hand.Right))
								otherPickupManager.currentPickupable = null;

							currentPickupable = currentPickupable.GetGrabbed(hand, gripAnchor);
						}
					}
					else if (!isGripping)
					{
						if (currentPickupable.IsObjectBeingHeld(hand))
						{
							currentPickupable.GetReleased(hand);
							currentPickupable = null;
							//handPhysics.ForceHandsToController();
						}
					}
					else if (!handDelegate.IsHandInRange())
					{
						if (currentPickupable.IsObjectBeingHeld(hand))
						{
							currentPickupable.GetReleased(hand);
							currentPickupable = null;
							//handPhysics.ForceHandsToController();
						}
					}
				}
				isGrippingPrevious = isGripping;
			}
		}

		public void OnGripExit(Collider col)
		{
			if (col.TryGetComponent(out PickupableObject pickupable))
			{
				if (currentPickupable == pickupable && !currentPickupable.IsObjectBeingHeld())// && !IsHandGripping() && currentPickupable.isObjectBeingHeld())
				{
					pickupable.GetReleased(hand);
					//handPhysics.ForceHandsToController();
					currentPickupable = null;
				}
			}
		}

		public bool IsHoldingObject()
		{
			return (currentPickupable != null) && currentPickupable.IsObjectBeingHeld();
		}

		private bool isGrippingPrevious = false;
		public bool IsHandGripping()
		{
			int gripCount = 0;
			bool[] fingers = new bool[5]
				{
					IsFingerGripping(Fingers.Thumb),
					IsFingerGripping(Fingers.Index),
					IsFingerGripping(Fingers.Middle),
					IsFingerGripping(Fingers.Ring),
					IsFingerGripping(Fingers.Pinky)
				};

			foreach(bool f in fingers)
			{
				if(f)
					gripCount++;
			}

			return gripCount > 2;
		}

		public void SetGrippingMaterial(bool isGripping)
		{
			// TO DO: see how to assign physics materials without creating new ones
			return;
			PhysicMaterial mat = (isGripping) ? grippingPhysicsMaterial : nonGrippingPhysicsMaterial;

			// exit if material is already correct gripped material.
			if(connectedColliders[0].material == mat)
				return;

			foreach(Collider c in connectedColliders)
			{
				c.material = mat;
			}
		}

		// checks if the controller is inputting the player pressing the buttons (reading physics driven fingers might make some pickupables impossible to grip)
		private bool IsFingerGripping(Fingers finger)
		{
			return fingers[(int)finger].GetFingerValueRaw() > MIN_GRIPPING_VALUE;
		}

		[ContextMenu("Set all child colliders")]
		private void SetColliders()
		{
			Collider[] colliders = GetComponentsInChildren<Collider>();
			connectedColliders = new List<Collider>();

			foreach(Collider c in colliders)
			{
				if(!c.isTrigger)
				{
					connectedColliders.Add(c);
					if(nonGrippingPhysicsMaterial != null)
						c.material = nonGrippingPhysicsMaterial;
				}
			}
		}
	}
}
