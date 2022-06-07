using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrillCover : MonoBehaviour
{
    // disable hinge joint spring when in down position
    // flip target position on this?
    [SerializeField] private float disableSpringLimit = 85f;

    [SerializeField] private Transform grillHood = null;

    [SerializeField] private HingeJoint hingeJoint = null;
    [SerializeField] private float upTargPos = 5.0f;
	[SerializeField] private float downTargPos = -100.0f;

	public bool IsHoodClosed { get; private set; } = false;

	// some particles when closed (smoke shooting out form under hood)
	// particles when hood opens (smokescreen)

	private void Start()
	{
		Vector3 ang = grillHood.transform.localEulerAngles;
		ang.x = hingeJoint.limits.min * -1f;
		grillHood.transform.localEulerAngles = ang;
	}

	private void Update()
	{
		JointSpring spr = hingeJoint.spring;
		spr.targetPosition = 0;
		if (Vector3.Dot(grillHood.forward, Vector3.up) > disableSpringLimit)
		{
			spr.targetPosition = downTargPos;
			IsHoodClosed = true;
		}
		else
		{
			spr.targetPosition = upTargPos;
			IsHoodClosed = false;
		}
		hingeJoint.spring = spr;
	}
}
