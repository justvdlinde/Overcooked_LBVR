using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreventRotation : MonoBehaviour
{
    [SerializeField] private Vector3 desiredRotation = new Vector3();
	public bool unparentObject = true;
	public bool doRotation = false;

	public ParticleSystem ps = null;

	private void Awake()
	{
		transform.parent = null;
	}

	
}
