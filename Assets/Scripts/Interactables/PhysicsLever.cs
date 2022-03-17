using PhysicsCharacter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PhysicsLever : MonoBehaviour
{
	[Header("Lever values")]
	[SerializeField] private float clickThreshold = 0.75f;
	[SerializeField] private UnityEvent onPress = null;
	[SerializeField] private UnityEvent onRelease = null;

	[SerializeField] private Tool toolHandle = null;

	[SerializeField] private Transform leverTransform = null;
	private HingeJoint hingeJoint = null;
	private float upperLimit = -90f;
	private float lowerLimit = 90f;

	[SerializeField] private float leverReactionSpots = 0.1f;

	public float offset = 0;

	private bool wasUp = false;

	private void Awake()
	{
		hingeJoint = GetComponent<HingeJoint>();
		if (hingeJoint != null)
		{
			upperLimit = hingeJoint.limits.max;
			lowerLimit = hingeJoint.limits.min;
		}
	}

	private void Update()
	{
		hingeJoint.useSpring = !toolHandle.IsBeingHeld();

		if (IsLeverDown() && wasUp)
		{
			onPress?.Invoke();
			wasUp = false;
		}
		if (!wasUp && IsLeverUp())
		{
			onRelease.Invoke();
			wasUp = true;
		}
	}

	private bool IsLeverUp()
	{
		return GetLeverValue01() < leverReactionSpots;
	}

	private bool IsLeverDown()
	{
		return GetLeverValue01() > 1f - leverReactionSpots;
	}

	private float GetLeverValue01()
	{
		return Mathf.InverseLerp(-1, 1, Vector3.Dot(transform.up, leverTransform.forward));
	}

	private void OnValidate()
	{
		hingeJoint = GetComponent<HingeJoint>();
		if(hingeJoint != null)
		{
			upperLimit = hingeJoint.limits.max;
			lowerLimit = hingeJoint.limits.min;
		}
	}
}
