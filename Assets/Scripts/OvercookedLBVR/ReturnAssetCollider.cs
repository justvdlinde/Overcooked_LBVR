using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnAssetCollider : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		ReturnableAsset asset = other.GetComponentInParent<ReturnableAsset>();

		if(asset != null)
		{
			asset.Return();
		}
	}
}
