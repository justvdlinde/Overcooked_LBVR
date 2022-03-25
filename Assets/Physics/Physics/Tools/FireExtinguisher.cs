using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireExtinguisher : MonoBehaviour
{
    // particle collision

    bool isActive = false;
	public Rigidbody rb = null;

	private void Update()
	{
		if(isActive)
		{
			rb.AddForce(-rb.transform.forward * 10f, ForceMode.Acceleration);
		}
	}
}
