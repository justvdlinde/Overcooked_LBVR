using System.Collections.Generic;
using UnityEngine;

namespace PhysicsCharacter
{
	public class MultiHandleDelegate : ToolPositionDelegate
	{
		[SerializeField] private List<ToolHandle> followTargets = null;

		private ToolHandle primaryToolHandle = null;
		private Hand primaryToolHandleHand = Hand.None;
		private ToolHandlePriority primaryToolHandlePriority = ToolHandlePriority.Lowest;

		private ToolHandle secondaryToolHandle = null;
		private Hand secondaryToolHandleHand = Hand.None;
		private ToolHandlePriority secondaryToolHandlePriority = ToolHandlePriority.Lowest;
		public Vector3 multiHandRotationOffset = new Vector3(0, 90, 90);
		public Vector3 leftHandRotationOffset = new Vector3(0, 0, 0);
		public Vector3 rightHandRotationOffset = new Vector3(0, 0, 0);


		private void Awake()
		{
			foreach(ToolHandle handle in followTargets)
			{
				handle.OnGrabbedCall += RegisterGrabbedHandle;
				handle.OnReleasedCall += DeregisterGrabbedHandle;
			}
		}

		private void RegisterGrabbedHandle(Hand hand, ToolHandle handle)
		{
			// registed first hand on tool as primary for positioning purposes
			if(primaryToolHandle == null)
			{
				primaryToolHandle = handle;
				primaryToolHandleHand = hand;
				primaryToolHandlePriority = handle.GetHandlePriority();
			}
			else if(primaryToolHandle != null && (int)handle.GetHandlePriority() > (int)primaryToolHandle.GetHandlePriority())
			{
				// swap hand if for some reason the hand wasn't correctly reset
				if(handle == primaryToolHandle && hand != primaryToolHandleHand)
					primaryToolHandleHand = hand;

				if(handle == primaryToolHandle && handle.GetHandlePriority() != primaryToolHandlePriority)
					primaryToolHandlePriority = handle.GetHandlePriority();

				if(handle == primaryToolHandle)
					return;

				// if priority of new handle is higher than currently grabbed handle, overwrite and move current primary to secondary
				// else register secondary
				if((int)handle.GetHandlePriority() > (int)primaryToolHandle.GetHandlePriority())
				{
					secondaryToolHandlePriority = primaryToolHandlePriority;
					secondaryToolHandleHand = primaryToolHandleHand;
					secondaryToolHandle = primaryToolHandle;

					primaryToolHandlePriority = handle.GetHandlePriority();
					primaryToolHandleHand = hand;
					primaryToolHandle = handle;
				}
			}

			// if primary has been set, skip next step
			if(hand == primaryToolHandleHand)
				return;

			if(secondaryToolHandle == null)
			{
				secondaryToolHandle = handle;
				secondaryToolHandleHand = hand;
				secondaryToolHandlePriority = handle.GetHandlePriority();
			}
			else if(secondaryToolHandle != null)
			{
				if((int)handle.GetHandlePriority() > (int)primaryToolHandle.GetHandlePriority())
				{
					ToolHandle buffer = primaryToolHandle;
					Hand h = primaryToolHandleHand;
					ToolHandlePriority p = primaryToolHandlePriority;

					primaryToolHandle = handle;
					primaryToolHandleHand = hand;
					primaryToolHandlePriority = handle.GetHandlePriority();

					secondaryToolHandle = buffer;
					secondaryToolHandleHand = h;
					secondaryToolHandlePriority = p;
					return;
				}

				// swap hand if for some reason the hand wasn't correctly reset
				if(handle == secondaryToolHandle && hand != secondaryToolHandleHand)
					secondaryToolHandleHand = hand;

				if(handle.GetHandlePriority() != secondaryToolHandlePriority)
					secondaryToolHandlePriority = handle.GetHandlePriority();

				if(handle == secondaryToolHandle)
					return;
				// if for some reason the handle still has higher priority than the primary, replace it
			}
		}

		private void DeregisterGrabbedHandle(Hand hand, ToolHandle handle)
		{
			if(hand == primaryToolHandleHand)
			{
				if(secondaryToolHandle != null)
				{
					primaryToolHandle = secondaryToolHandle;
					primaryToolHandleHand = secondaryToolHandleHand;
					primaryToolHandlePriority = secondaryToolHandlePriority;

					secondaryToolHandle = null;
					secondaryToolHandleHand = Hand.None;
					secondaryToolHandlePriority = ToolHandlePriority.Lowest;
				}
				else
				{
					primaryToolHandle = null;
					primaryToolHandleHand = Hand.None;
					primaryToolHandlePriority = ToolHandlePriority.Lowest;

				}
			}
			else if(hand == secondaryToolHandleHand)
			{
				secondaryToolHandle = null;
				secondaryToolHandleHand = Hand.None;
				secondaryToolHandlePriority = ToolHandlePriority.Lowest;
			}
		}

		public override Vector3 GetAnchorPosition()
		{
			if(primaryToolHandlePriority == secondaryToolHandlePriority)
				return GetAverageAnchorPosition();
			else if(primaryToolHandle != null)
				return primaryToolHandle.localTransformMirror.GetWorldPosition();
			else
				return GetAverageAnchorPosition();
		}

		public override Vector3 GetPosition()
		{
			if(primaryToolHandlePriority == secondaryToolHandlePriority)
				return GetAveragePosition();
			else if(primaryToolHandle != null)
				return primaryToolHandle.transform.position;
			else
				return GetAveragePosition();
		}

		public override Quaternion GetRotation()
		{
			if(primaryToolHandlePriority == secondaryToolHandlePriority)
				return GetAverageQuaternion();
			else if(primaryToolHandle != null && secondaryToolHandle != null)
				return GetGripRotation();
			else 
				return GetAverageQuaternion();
		}

		public Quaternion GetGripRotation()
		{
			Vector3 target = secondaryToolHandle.transform.position - primaryToolHandle.transform.position;
			Quaternion lookRotation = Quaternion.LookRotation(target, Vector3.Lerp(primaryToolHandle.transform.up, secondaryToolHandle.transform.up, 0.5f));
			lookRotation *= Quaternion.Euler(multiHandRotationOffset);

			Quaternion avg = GetAverageQuaternion();

			return lookRotation;
		}

		private Vector3 GetAveragePosition()
		{
			Vector3 pos = Vector3.zero;

			int count = 0;
			foreach(ToolHandle t in followTargets)
			{
				if(t.IsObjectBeingHeld())
				{
					pos += (t.transform.position);
					count++;
				}
			}

			pos = pos / count;

			return pos;
		}

		private Vector3 GetAverageAnchorPosition()
		{
			Vector3 pos = Vector3.zero;

			int count = 0;
			foreach(ToolHandle t in followTargets)
			{
				if(t.IsObjectBeingHeld())
				{
					pos += (t.localTransformMirror.GetWorldPosition());
					count++;
				}
			}

			pos = pos / count;

			return pos;
		}

		private Quaternion GetAverageQuaternion()
		{
			int amount = 0;
			Quaternion returnValue = Quaternion.identity;

			foreach(ToolHandle t in followTargets)
			{
				if(t.IsObjectBeingHeld())
				{
					amount++;
					returnValue = Quaternion.Slerp(returnValue, t.transform.rotation, 1.0f / (float)amount);
				}
			}

			return returnValue * Quaternion.Euler(((primaryToolHandleHand == Hand.Right) ? rightHandRotationOffset : leftHandRotationOffset));
		}
	}
}