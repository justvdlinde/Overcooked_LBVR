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

	[SerializeField] private Transform leverTransform = null;
	private HingeJoint hingeJoint = null;
	private float upperLimit = -90f;
	private float lowerLimit = 90f;

	public float offset = 0;

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
		float ang = Vector3.Dot(transform.up, leverTransform.forward);
		Debug.Log($"Vector3.Dot(transform.up, leverTransform.forward {Mathf.InverseLerp(-1, 1, ang)}" );
		//Debug.Log($"GetLeverValue01() {GetLeverValue01()} lowerLimit {lowerLimit + 180f} upperLimit {upperLimit + 180f} localAng.x {Mathf.Abs(leverTransform.localEulerAngles.x * Mathf.Deg2Rad)}" );
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
