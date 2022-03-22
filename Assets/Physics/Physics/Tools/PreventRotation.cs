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

	private void Update()
	{
		if(doRotation)
			transform.eulerAngles = desiredRotation;
	}

	List<ParticleCollisionEvent> cEvents = null;
	private void OnParticleCollision(GameObject other)
	{
		int numCols = ps.GetCollisionEvents(other, cEvents);
	}
}
