using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsCharacter
{
	//[RequireComponent(typeof(Rigidbody))]
	public class Tool : MonoBehaviour
	{
		public bool MarkedForDestruction { get; protected set; }

		[SerializeField] private List<ToolHandle> toolHandles = null;
		[SerializeField] private ToolPositionDelegate toolTransformDelegate = null;
		public DummyToolHandle dummyToolHandle = null;
		private int heldHandles = 0;

		// physics movement section
		[SerializeField] protected Rigidbody rigidBody = null;
		[SerializeField] protected float slowDownVelocity = 0.75f;
		[SerializeField] protected float slowDownRotation = 0.75f;

		[SerializeField] protected float maxPositionChange = 500f;
		[SerializeField] protected float maxRotationChange = 720f;
		[SerializeField] protected float maxAngularVelocity = 7f;

		[SerializeField] protected ClippingCheckerOnSpawn clipChecker = null;
		[SerializeField] protected PhotonView photonView = null;
		[SerializeField] protected PhotonView rootPhotonView = null;
		public PhotonView RootPhotonView => rootPhotonView;

		private Vector3 targetPos = Vector3.zero;

		private Quaternion targetRot = Quaternion.identity;
		private float maxGripDistance = 2.0f;
		public bool forcePosition = true;

		public Action OnLocalPickupEvent = null;
		public Action OnLocalDropEvent = null;

		public List<Transform> childrenToUnparentOnDestroy = new List<Transform>();

		private void OnDestroy()
		{
			MarkedForDestruction = true;

			foreach (var item in childrenToUnparentOnDestroy)
			{
				if (transform == null)
					continue;
				if (item == null)
					continue;
				if (item.parent == transform)
					item.parent = null;
			}
			childrenToUnparentOnDestroy.Clear();

			foreach (ToolHandle h in toolHandles)
			{
				if (h.IsObjectBeingHeld())
					h.GetReleased(h.HeldHand);
			}
		}

		public Vector3 GetFollowPos(Hand hand)
		{
			foreach (ToolHandle t in toolHandles)
			{
				if (hand == Hand.Right && t.allowRightGrip && !t.UseHandleHandedness)
					return t.localTransformMirror.GetWorldPosition();

				if (hand == Hand.Left && t.allowLeftGrip && !t.UseHandleHandedness)
					return t.localTransformMirror.GetWorldPosition();


				if (hand == t.preferredToolHand)
					return t.localTransformMirror.GetWorldPosition();

			}

			return toolHandles[0].localTransformMirror.GetWorldPosition();
			;
		}

		public ToolHandle GetTargToolhandle(Hand hand)
		{
			foreach (ToolHandle t in toolHandles)
			{
				if (hand == Hand.Right && t.allowRightGrip && !t.UseHandleHandedness)
					return t;

				if (hand == Hand.Left && t.allowLeftGrip && !t.UseHandleHandedness)
					return t;


				if (hand == t.preferredToolHand)
					return t;

			}

			return toolHandles[0];
			;
		}

		public Quaternion GetFollowQuat(Hand hand)
		{
			return toolTransformDelegate.GetRotation();
		}

		public Hand GetHeldHand()
		{
			foreach (ToolHandle handle in toolHandles)
			{
				if (handle.IsObjectBeingHeld())
					return handle.HeldHand;
			}
			return Hand.None;
		}

		private void Awake()
		{
			if(rigidBody != null)
				rigidBody.maxAngularVelocity = maxAngularVelocity;

			foreach(ToolHandle toolHandle in toolHandles)
			{
				toolHandle.parentTool = this;
				toolHandle.OnGrabbedCall += OnGrabbedCallback;
				toolHandle.OnReleasedCall += OnReleasedCallback;
			}

			if (photonView == null)
				photonView = GetComponent<PhotonView>();
		}

		protected virtual void Start()
		{
			// empty intentionally
			if (PhysicsPlayerBlackboard.Instance == null)
				gameObject.SetActive(false);
		}

        protected virtual void Update()
		{
			//if(!IsBeingHeld())
			//{
			//	rigidBody.useGravity = true;
			//}

			// disable this when rotating
			if(!PhysicsPlayerBlackboard.Instance.isFading)
			{
				foreach(ToolHandle t in toolHandles)
				{
					t.CheckDistance(maxGripDistance);
				}
			}

			//if(!IsBeingHeld() && (heldHandles <= 0))
			//{
			//	float dist = Vector3.Distance(transform.position, KHS_PhysicsPlayerBlackboard.Instance.headAnchor.position);
			//	if(dist > 2.5f)
			//		ReleaseTool();
			//}
		}

		protected virtual void FixedUpdate()
		{
			foreach (var item in toolHandles)
			{
				if (Vector3.Distance(item.transform.position, item.localTransformMirror.GetWorldPosition()) < 0.015f)
					item.HandleIsCloseToToolPos = true;
			}

			if (heldHandles > 0 && !PhysicsPlayerBlackboard.Instance.isFading)
			{
				if(!disablePosition)
					MoveUsingPhysics();
				if(!disableRotation)
					RotateUsingPhysics();
			}
			else if(heldHandles > 0)
			{
				transform.rotation = toolTransformDelegate.GetRotation();
				targetRot = toolTransformDelegate.GetRotation();

				ToolHandle pickedupHandle = null;
				int currentHandlePrio = -10;
				for(int i = 0; i < toolHandles.Count; i++)
				{
					if(toolHandles[i].IsObjectBeingHeld())
					{
						if((int)toolHandles[i].GetHandlePriority() > currentHandlePrio)
						{
							pickedupHandle = toolHandles[i] as ToolHandle;
							currentHandlePrio = (int)pickedupHandle.GetHandlePriority();
						}
					}
				}
				if(pickedupHandle == null)
					return;
				transform.position = pickedupHandle.transform.position;
				Vector3 diff = (toolTransformDelegate.GetPosition()) - (rigidBody.worldCenterOfMass + (toolTransformDelegate.GetAnchorPosition() - rigidBody.worldCenterOfMass));
				transform.position += diff;
				targetPos = transform.position;
			}
		}

		public void PickupTool(Hand hand)
		{
			if(toolHandles == null || toolHandles.Count <= 0 || IsBeingHeld(hand))
				return;

			ToolHandle pickedupHandle = toolHandles[0].GetGrabbed(hand, PhysicsPlayerBlackboard.Instance.GetFollowTarget(hand)) as ToolHandle;
			if(pickedupHandle == null)
				return;

			if(clipChecker != null)
				clipChecker.EnableClippingChecker(hand);

			pickedupHandle.OnReleasedCall += ReleaseCallback;
			if(pickedupHandle.OtherToolHandle != null)
				pickedupHandle.OtherToolHandle.OnReleasedCall += ReleaseCallback;

			PhysicsPlayerBlackboard.Instance.PickupItem(hand, pickedupHandle);

			//transform.rotation = toolTransformDelegate.GetRotation();
			targetRot = toolTransformDelegate.GetRotation();

			//transform.position = pickedupHandle.transform.position;
			//Vector3 diff = (toolTransformDelegate.GetPosition()) - (rigidBody.worldCenterOfMass + (toolTransformDelegate.GetAnchorPosition() - rigidBody.worldCenterOfMass));
			//transform.position += diff;
			targetPos = transform.position;


			//gameObject.SetActive(true);
		}

		protected void ForcePosition()
		{
			//transform.rotation = toolTransformDelegate.GetRotation();
			////rigidBody.MoveRotation(toolTransformDelegate.GetRotation());
			//targetRot = toolTransformDelegate.GetRotation();

			if (!forcePosition)
				return;
			ToolHandle pickedupHandle = null;
			int currentHandlePrio = -10;
			for (int i = 0; i < toolHandles.Count; i++)
			{
				if (toolHandles[i].IsObjectBeingHeld())
				{
					if ((int)toolHandles[i].GetHandlePriority() > currentHandlePrio)
					{
						pickedupHandle = toolHandles[i] as ToolHandle;
						currentHandlePrio = (int)pickedupHandle.GetHandlePriority();
					}
				}
			}
			if (pickedupHandle == null)
				return;

			Vector3 pos = pickedupHandle.transform.position;
			Vector3 diff = (toolTransformDelegate.GetPosition()) - (rigidBody.worldCenterOfMass + (toolTransformDelegate.GetAnchorPosition() - rigidBody.worldCenterOfMass));
			rigidBody.MovePosition(pos);
			//transform.position += diff;
			targetPos = rigidBody.position;
		}

		// move this to a distance check in update
		public void ReleaseCallback(Hand hand, ToolHandle handle)
		{
			if(!IsBeingHeld())
			{
				if(heldHandles <= 0)
				{
					//gameObject.SetActive(false);
					handle.OnReleasedCall -= ReleaseCallback;
					if(handle.OtherToolHandle != null)
						handle.OtherToolHandle.OnReleasedCall -= ReleaseCallback;

					if(clipChecker != null)
						clipChecker.ResetColliderObject();
				}
			}
		}

		public void ReleaseTool(Hand hand = Hand.None)
		{
			foreach(ToolHandle t in toolHandles)
			{
				t.ForceRelease();

				t.transform.parent = transform;
				t.transform.localPosition = t.localTransformMirror.localPosition;
				t.transform.localRotation = t.localTransformMirror.localRotation;
			}

			//gameObject.SetActive(false);
			if(hand == Hand.None)
			{
				PhysicsPlayerBlackboard.Instance.DropItem(Hand.Right);
				PhysicsPlayerBlackboard.Instance.DropItem(Hand.Left);

				PhysicsPlayerBlackboard.Instance.SetToolAndHandCollisions(Hand.Right, false);
				PhysicsPlayerBlackboard.Instance.SetToolAndHandCollisions(Hand.Left, false);
				
			}
			else
			{
				PhysicsPlayerBlackboard.Instance.SetToolAndHandCollisions(hand, false);

				PhysicsPlayerBlackboard.Instance.DropItem(hand);
			}

			photonView.TransferOwnership(-1);
			rootPhotonView.TransferOwnership(-1);
		}

		public bool IsBeingHeld()
		{
			foreach(ToolHandle h in toolHandles)
			{
				if(h.IsObjectBeingHeld())
					return true;
			}
			return false;
		}

		public bool IsBeingHeldRemote()
		{
			foreach (ToolHandle h in toolHandles)
			{
				if (h.IsHeldByRemote())
					return true;
			}
			return false;
		}

		public bool IsBeingHeld(Hand hand)
		{
			foreach(ToolHandle h in toolHandles)
			{
				if(h.IsObjectBeingHeld(hand))
					return true;
			}
			return false;
		}

		public bool disablePosition = false;
		public bool disableRotation = false;

		protected virtual void OnGrabbedCallback(Hand hand, ToolHandle toolHandle)
		{
			if(heldHandles < 0)
				heldHandles = 0;

			if(heldHandles == 0)
			{
				OnLocalPickupEvent?.Invoke();
			}

			rigidBody.centerOfMass = rigidBody.transform.InverseTransformPoint(toolTransformDelegate.GetAnchorPosition());
			heldHandles++;

			rigidBody.useGravity = false;

			maxPositionChange = 10f;

			photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
			rootPhotonView.TransferOwnership(PhotonNetwork.LocalPlayer);
			photonView.RPC(nameof(GrabbedRPC), RpcTarget.Others);
		}

		[PunRPC]
		protected virtual void GrabbedRPC(PhotonMessageInfo info)
        {
			Debug.Log("GrabbedRPC, disabling gravity");
			foreach (ToolHandle handle in toolHandles)
				handle.isGrabbedByRemote = true;
			rigidBody.useGravity = false;
		}

		protected virtual void OnReleasedCallback(Hand hand, ToolHandle toolHandle)
		{
			if (rigidBody == null)
				return;

			if (heldHandles > 0)
				heldHandles--;

			if(heldHandles <= 0)
			{
				rigidBody.useGravity = true;
				rigidBody.ResetCenterOfMass();
				OnLocalDropEvent?.Invoke();
			}
			else
				rigidBody.centerOfMass = rigidBody.transform.InverseTransformPoint(toolTransformDelegate.GetAnchorPosition());

			toolHandle.HandleIsCloseToToolPos = false;

			if (!MarkedForDestruction)
			{
				toolHandle.transform.parent = transform;
				toolHandle.transform.localPosition = toolHandle.localTransformMirror.localPosition;
				toolHandle.transform.localRotation = toolHandle.localTransformMirror.localRotation;
			}

			//photonView.TransferOwnership(-1);
			photonView.RPC(nameof(ReleasedRPC), RpcTarget.Others, rigidBody.useGravity);
		}

		[PunRPC]
		protected virtual void ReleasedRPC(bool useGravity, PhotonMessageInfo info)
		{
			foreach (ToolHandle handle in toolHandles)
				handle.isGrabbedByRemote = false;
			rigidBody.useGravity = useGravity;
		}

		protected virtual void MoveUsingPhysics()
		{
			rigidBody.velocity *= slowDownVelocity;

			Vector3 newVelocity = FindNewVelocity();
			if(IsValidVelocity(newVelocity.x))
			{
				float maxChange = maxPositionChange * Time.deltaTime;
				rigidBody.velocity = Vector3.MoveTowards(rigidBody.velocity, newVelocity, maxChange);
			}
		}

		protected Vector3 FindNewVelocity()
		{
			maxPositionChange = Mathf.MoveTowards(maxPositionChange, 500f, 10f);
			//				world pos of tool handle		world pos of tool itself	world pos of relative pos where handle was attached on spawn minus tool pos to get the offset
			Vector3 diff = (toolTransformDelegate.GetPosition()) - (rigidBody.worldCenterOfMass + (toolTransformDelegate.GetAnchorPosition() - rigidBody.worldCenterOfMass));
			return diff / Time.deltaTime;
		}
		protected void RotateUsingPhysics()
		{
			rigidBody.angularVelocity *= slowDownRotation;

			Vector3 newAngularVelocity = FindNewAngularVelocity();

			if(IsValidVelocity(newAngularVelocity.x))
			{
				float maxChange = maxRotationChange * Time.deltaTime;
				rigidBody.angularVelocity = Vector3.MoveTowards(rigidBody.angularVelocity, newAngularVelocity, maxChange);
			}
		}

		protected Vector3 FindNewAngularVelocity()
		{
			Quaternion diff = toolTransformDelegate.GetRotation() * Quaternion.Inverse(rigidBody.rotation);
			diff.ToAngleAxis(out float angle, out Vector3 rotationAxis);

			if(angle > 180f)
				angle -= 360f;

			return (rotationAxis * angle * Mathf.Deg2Rad) / (Time.deltaTime);
		}

		protected bool IsValidVelocity(float x)
		{
			return !float.IsNaN(x) && !float.IsInfinity(x);
		}

		public bool IsToolReleasedCompletely()
		{
			bool result = true;

			foreach(ToolHandle handle in toolHandles)
			{
				result = handle.releasedLeftHand || handle.releasedRightHand;
				if(result == false)
					return false;
			}

			return result;
		}
	}
}
