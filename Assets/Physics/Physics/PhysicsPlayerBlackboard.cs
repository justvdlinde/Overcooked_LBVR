using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PhysicsCharacter
{
	// ENTIRE CLASS NEEDS PROPERTIES FOR GETTING. NO SETTING
	public class PhysicsPlayerBlackboard : MonoBehaviour
	{
		public static PhysicsPlayerBlackboard Instance = null;

		[SerializeField] private PhysicsCharacterController characterController = null;

		public Rigidbody playerRigidBody = null;

		public Transform headAnchor = null;
		public Transform leftController = null;
		public Transform rightController = null;
		public Transform teleportAnchor = null;

		public PhysicsPickupManager leftHandPickupManager = null;
		public PhysicsPickupManager rightHandPickupManager = null;

		public Hand PreferredHand => characterController.preferredHand;

		public bool leftIsHoldingMovementTool = false;
		public bool rightIsHoldingMovementTool = false;
		public bool isHoldingMovementTool => leftIsHoldingMovementTool || rightIsHoldingMovementTool;

		public bool isLeftHandGripping = false;
		public bool isRightHandGripping = false;

		public static int INGREDIENT_MASK = 6;
		public static int TOOL_LAYER_MASK = 26;
		public static int RIGHT_HAND_LAYER_MASK = 29;
		public static int LEFT_HAND_LAYER_MASK = 30;

		// teleporter stuff
		public Action<float> snapTurnEvent = null;
		public Action<Vector3, Vector3> playerTeleportEvent = null;
		public bool IsAimingTeleport => characterController.isAimingTeleport;
		public bool HasValidTeleportTarget => PhysArcPredictor.HasCollision && (PhysArcPredictor.RaycastHit.collider.gameObject.layer == LayerMask.NameToLayer("Water") || PhysArcPredictor.RaycastHit.collider.gameObject.layer == LayerMask.NameToLayer("VRTeleportWalkable"));
		public PhysTrajectoryPredictor PhysArcPredictor { get; private set; } = null;
		[SerializeField] private LayerMask teleporterLayerMask = 0;

		// fade box stuff
		public bool isFading = false;

		// list of tools
		// enable/equip calls
		// spawn tools on awake player
		// 

		private void Awake()
		{
			if(Instance == null)
				Instance = this;
			else
				enabled = false;

			PhysArcPredictor = new PhysTrajectoryPredictor(30, 0.05f, Physics.gravity, 7.5f, true, (int)teleporterLayerMask);

			playerTeleportEvent += OnPlayerTeleport;
		}

		private void OnPlayerTeleport(Vector3 point, Vector3 direction)
		{
			//float yAxisController = transform.eulerAngles.y + direction;

			if(!isFading)
				characterController.OnTeleportPlayer(point, direction);
		}

		public Transform GetFollowTarget(Hand hand)
		{
			if(hand == Hand.Left)
				return leftController;
			else
				return rightController;
		}

		public bool IsHandGripping(Hand hand)
		{
			if(hand == Hand.Left)
				return isLeftHandGripping;
			else
				return isRightHandGripping;
		}

		public void PickupItem(Hand hand, PickupableObject obj)
		{
			if(hand == Hand.Left)
				leftHandPickupManager.ForceSetPickupable(obj);
			else
				rightHandPickupManager.ForceSetPickupable(obj);
		}

		public void DropItem(Hand hand)
		{
			if(hand == Hand.Left)
				leftHandPickupManager.ForceDropItem();
			else
				rightHandPickupManager.ForceDropItem();

		}

		public void SetToolAndHandCollisions(Hand hand, bool collisionsIgnored)
		{
			if(hand == Hand.Left)
			{
				Physics.IgnoreLayerCollision(INGREDIENT_MASK, LEFT_HAND_LAYER_MASK, collisionsIgnored);

				Physics.IgnoreLayerCollision(TOOL_LAYER_MASK, LEFT_HAND_LAYER_MASK, collisionsIgnored);
			}
			else
			{
				Physics.IgnoreLayerCollision(INGREDIENT_MASK, RIGHT_HAND_LAYER_MASK, collisionsIgnored);

				Physics.IgnoreLayerCollision(TOOL_LAYER_MASK, RIGHT_HAND_LAYER_MASK, collisionsIgnored);
			}
		}
	}
}