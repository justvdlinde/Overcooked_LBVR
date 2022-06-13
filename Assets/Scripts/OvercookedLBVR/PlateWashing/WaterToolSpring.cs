using PhysicsCharacter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterToolSpring : MonoBehaviour
{
    public SpringJoint spring;

	private float springval = 1f;

	public Tool connectedTool = null;

	public LineRenderer lineRenderer = null;

	public Transform line = null;

	private void Awake()
	{
		springval = spring.spring;
	}

	private void Update()
	{
		if(connectedTool.IsBeingHeld())
		{
			if (Vector3.Distance(connectedTool.transform.position, transform.position) > 1f)
			{
				connectedTool.GetTargToolhandle(connectedTool.GetHeldHand()).GetReleased(connectedTool.GetHeldHand());
			}
		}

		if (connectedTool.IsBeingHeld())
			DisableSpring();
		else
			EnableSpring();

		List<Vector3> positions = new List<Vector3>();
		positions.Add(line.position);

		foreach (Transform child in line)
		{
			positions.Add(child.position);
		}

		positions.Add(connectedTool.transform.position);
		lineRenderer.SetPositions(positions.ToArray());
	}

	public void EnableSpring()
	{
		spring.spring = springval;
	}

	public void DisableSpring()
	{
		spring.spring = 0f;
	}
}
