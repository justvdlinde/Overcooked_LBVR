using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnableAssetDispenser : MonoBehaviour
{
	[SerializeField] private Transform returnPosition = null;

	[SerializeField] private ReturnableAsset connectedAsset = null;

	private Vector3 previousPos;
	private float returnTimer = 0;
	[SerializeField] private float idleReturnTimer = 10f;
	[SerializeField] private float returnRadius = 0.25f;

	private void Awake()
	{
		connectedAsset.Init(this);
	}

	private void Update()
	{
		float distance = Vector3.Distance(connectedAsset.transform.position, returnPosition.position);


		if(distance > returnRadius)
		{
			if (previousPos == connectedAsset.transform.position)
				returnTimer += Time.deltaTime;
			else
				returnTimer = 0f;

			if(returnTimer >= idleReturnTimer)
			{
				connectedAsset.Return();
				returnTimer = 0f;
			}
		}
		else
			returnTimer = 0f;



		previousPos = connectedAsset.transform.position;
	}

	public Vector3 GetReturnPos()
	{
		return returnPosition.position;
	}

	public Quaternion GetReturnRot()
	{
		return returnPosition.rotation;
	}
}
