using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsCharacter
{
	[RequireComponent(typeof(Rigidbody))]
	public class PhysicsHandsController : MonoBehaviour
	{
		[SerializeField] private Rigidbody rigidBody = null;

		[SerializeField] private float physicsRange = 0.1f;
		[SerializeField] private LayerMask physicsMask = 0;
		[SerializeField] private PhysicsPickupManager pickupManager = null;

		private float slowDownVelocity = 0.75f;
		private float slowDownRotation = 0.75f;

		private float maxPositionChange = 180f;
		private float maxRotationChange = 5000f;

		private Vector3 targetPos = Vector3.zero;
		private Quaternion targetRot = Quaternion.identity;

		private Hand hand = Hand.None;
		private HandsDataDelegate handsDelegate = null;

		private void Start()
		{
			hand = pickupManager.Hand;

			if(hand == Hand.Left)
				handsDelegate = HandsDataDelegate.LeftHandInstance;
			else if(hand == Hand.Right)
				handsDelegate = HandsDataDelegate.RightHandInstance;

			rigidBody.maxAngularVelocity = 50f;
			MoveUsingTransform();
			RotateUsingTransform();

			//KHS_PhysicsPlayerBlackboard.Instance.AddFixedUpdateListener(new KHS_PhysicsPlayerBlackboard.FixedUpdatePrio(OnFixedUpdate, 2));
		}

		private void Update()
		{
			if(handsDelegate == null)
				return;

			targetPos = handsDelegate.GetFollowPosition();
			targetRot = handsDelegate.GetFollowRotation();
			if(IsHoldingObject() || !WithinPhysicsRange())
			{
				if(IsHoldingObject())
				{
					rigidBody.isKinematic = true;
					rigidBody.constraints = RigidbodyConstraints.FreezeAll;
				}


				MoveUsingTransform();
				RotateUsingTransform();
			}
		}

		//private void LateUpdate()
		//{
		//	if(IsHoldingObject() || !WithinPhysicsRange())
		//	{
		//		MoveUsingTransform();
		//		RotateUsingTransform();
		//	}
		//}

		private void FixedUpdate()
		{
			if(!IsHoldingObject() && WithinPhysicsRange())
			{
				rigidBody.isKinematic = false;

				rigidBody.constraints = RigidbodyConstraints.None;

				MoveUsingPhysics();
				RotateUsingPhysics();
			}
			else
			{
				MoveUsingTransform();
				RotateUsingTransform();
			}

			if (Vector3.Distance(transform.position, targetPos) > 1.0f)
				MoveUsingTransform();
		}

		public bool IsHoldingObject()
		{
			// work with pickup script here
			return pickupManager.IsHoldingObject() || handsDelegate.IsHoldingObject;
		}

		public bool WithinPhysicsRange()
		{
			return Physics.CheckSphere(transform.position, physicsRange, physicsMask, QueryTriggerInteraction.Ignore);
		}

		private void MoveUsingPhysics()
		{
			rigidBody.velocity *= slowDownVelocity;

			Vector3 newVelocity = FindNewVelocity();

			if(IsValidVelocity(newVelocity.x))
			{
				float maxChange = maxPositionChange * Time.deltaTime;

				rigidBody.velocity = Vector3.MoveTowards(rigidBody.velocity, newVelocity, maxChange);
			}
		}

		private Vector3 FindNewVelocity()
		{
			Vector3 diff = targetPos - rigidBody.position;
			return diff / Time.deltaTime;
		}

		private Vector3 FindNewVelocity(Vector3 t)
		{
			Vector3 diff = t - rigidBody.position;
			return diff / Time.deltaTime;
		}

		private void RotateUsingPhysics()
		{
			rigidBody.angularVelocity *= slowDownRotation;

			Vector3 newAngularVelocity = FindNewAngularVelocity();

			if(IsValidVelocity(newAngularVelocity.x))
			{
				float maxChange = maxRotationChange * Time.deltaTime;
				rigidBody.angularVelocity = Vector3.MoveTowards(rigidBody.angularVelocity, newAngularVelocity, maxChange);
			}
		}

		private Vector3 FindNewAngularVelocity()
		{
			Quaternion diff = targetRot * Quaternion.Inverse(rigidBody.rotation);
			diff.ToAngleAxis(out float angle, out Vector3 rotationAxis);

			if(angle > 180f)
				angle -= 360f;

			return (rotationAxis * angle * Mathf.Deg2Rad) / (Time.deltaTime);
		}

		private bool IsValidVelocity(float x)
		{
			return !float.IsNaN(x) && !float.IsInfinity(x);
		}
		
		private void MoveUsingTransform()
		{ 
			rigidBody.velocity = Vector3.zero;
			rigidBody.transform.position = (handsDelegate.GetFollowPosition());
		}

		public void ForceHandsToController()
		{
			MoveUsingTransform();
			//rigidBody.velocity = Vector3.zero;
			//rigidBody.position = handsDelegate.GetControllerPosition();
		}

		private void RotateUsingTransform()
		{
			rigidBody.angularVelocity = Vector3.zero;
			rigidBody.transform.rotation = (handsDelegate.GetFollowRotation());
			//rigidBody.MoveRotation(Quaternion.Slerp(transform.rotation, handsDelegate.GetFollowRotation(), Time.deltaTime * 25f));
		}

		private void OnDrawGizmos()
		{
			Gizmos.DrawWireSphere(transform.position, physicsRange);
		}

		private void Awake()
		{
			if(rigidBody != null)
				rigidBody.useGravity = false;
		}
	}
}