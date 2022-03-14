using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClippingPlaneSetter : MonoBehaviour
{
	public Camera cam = null;
	public LayerMask waterLayer = 0;
	public float farValue = 30f;

	public GameObject up, down;

	private void Update()
	{
		if(Physics.Raycast(transform.position, Vector3.up, 1000f, waterLayer))
		{
			cam.farClipPlane = farValue;
			up.SetActive(false);
			down.SetActive(true);
		}
		else
		{
			up.SetActive(true);
			down.SetActive(false);
			cam.farClipPlane = 1000f;
		}
	}
}
