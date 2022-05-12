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

		if(transform.localScale.y < 0)
		{
			Vector3 localScale = transform.localScale;
			localScale.y *= -1;
			transform.localScale = localScale;
		}

		transform.position = returnDispenser.GetReturnPos();
		transform.rotation = returnDispenser.GetReturnRot();
	}
}
