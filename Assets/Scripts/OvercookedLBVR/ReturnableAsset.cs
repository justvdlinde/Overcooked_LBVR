using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnableAsset : MonoBehaviour
{
	private ReturnableAssetDispenser returnDispenser = null;
	[SerializeField] private Rigidbody rb = null;

	public void Init(ReturnableAssetDispenser r)
	{
		returnDispenser = r;
	}

    public void Return()
	{
		if(rb != null)
		{
			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
		}

		transform.position = returnDispenser.GetReturnPos();
		transform.rotation = returnDispenser.GetReturnRot();
	}
}
