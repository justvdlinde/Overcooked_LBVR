using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsCharacter
{
	// This class controls individual fingers.
	public class FingerController : MonoBehaviour
	{
		[SerializeField] private Hand hand = Hand.None;
		[SerializeField] private PhysicsFingerController fingerController = null;

		[SerializeField] private Fingers finger = Fingers.None;
		[SerializeField] private Animator animator = null;

		[SerializeField] private Transform fingerTip = null;

		public Fingers Finger { get { return Finger; } }
		public FingerPoses currentPose { get; private set; } = FingerPoses.Neutral;
		public FingerPoses previousPose { get; private set; } = FingerPoses.Neutral;

		public float currentInputValue { get; private set; } = 0.0f;
		private float previousTarget = 0.0f;
		[SerializeField] private LayerMask frozenCheckLayers = 0;

		public float currentVal { get; private set; } = 0.0f;
		
		public void SetFingerPose(FingerPoses pose, float target = 0.0f)
		{
			currentInputValue = target;

			float targetValue = 0.0f;
			switch(pose)
			{
				default:
					targetValue = target;
					break;
				case FingerPoses.Stretched:
					targetValue = -0.5f;
					break;
			}

			bool isFrozen = fingerController.canFreeze && fingerController.isFrozen;
			float fingerCap = (isFrozen) ? fingerController.fingerUpperRange : 1.0f;
			fingerCap = Mathf.Round((fingerCap * 100f)) * 0.01f;

			HandsDataDelegate handDelegate = HandsDataDelegate.GetHandedInstance(hand);
			if(handDelegate.IsHoldingObject)
			{
				// this freeze is determined by tool handle grabbing
				targetValue = Mathf.Min(handDelegate.GetFingerValue(finger), targetValue);
			}
			else if(isFrozen)
			{
				// this freeze is physics driven when no tool is held
				targetValue = Mathf.Min(fingerCap, targetValue);
			}

			float targetV100 = (targetValue * 100f);
			targetValue = Mathf.Round((targetV100)) * 0.01f;

			float fingerVal = animator.GetFloat("FingerFloat");
			float fingerV100 = (fingerVal * 100f);
			fingerVal = Mathf.Round((fingerV100)) * 0.01f;

			// convoluted block that enabled rotational freezing when needed for physics
			fingerVal = Mathf.Lerp(fingerVal, targetValue, 40f * Time.deltaTime);

			previousTarget = targetValue;
			fingerVal = Mathf.Clamp(fingerVal, -0.5f, 1f);
			animator.SetFloat("FingerFloat", fingerVal);
			currentVal = fingerVal;

			if(currentPose != pose)
			{
				previousPose = currentPose;
				currentPose = pose;
			}
		}

		public void ResetFingerInDelegate()
		{
			HandsDataDelegate.GetHandedInstance(hand).ResetFinger(finger);
		}
	}
}
