using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Attributes;

public class PlateDirtySpot : MonoBehaviour
{

	float cleanTime = 1.0f;
	float cleanProgress = 0.0f;
	public bool IsCleaned => cleanProgress >= cleanTime;

	[SerializeField] private Transform graphics = null;
	[SerializeField] private ParticleSystem isCleanedParticles;

	[SerializeField] private BoxCollider col = null;

	public bool isWashing = false;

	private void Update()
	{
		if (isWashing)
			DoCleanSpot();
	}

	// auto init to dirty
	private void OnEnable()
	{
		cleanProgress = 0.0f;
		graphics.localScale = Vector3.one;
		isWashing = false;
		if (col == null)
			col = GetComponent<BoxCollider>();
		col.enabled = true;
		graphics.gameObject.SetActive(true);
	}

	public void DoCleanSpot()
	{
		if (IsCleaned)
			return;
		// communicate these values over net? might be unneeded
		if (cleanProgress < cleanTime)
		{
			cleanProgress += Time.deltaTime;
			// emit some particles here to communicate function
		}
		else
			cleanProgress = 1.0f;

		float scaleTarget = Mathf.Lerp(1.0f, 0.1f, cleanProgress / cleanTime);
		graphics.localScale = Vector3.one * scaleTarget;

		if (cleanProgress >= cleanTime)
			DoClean();
	}

	private void DoClean()
	{
		// some RPC communication here
		isCleanedParticles.Play();
		col.enabled = false;
		graphics.gameObject.SetActive(false);
	}
}
