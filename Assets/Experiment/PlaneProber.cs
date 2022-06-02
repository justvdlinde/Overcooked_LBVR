using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Attributes;

public class PlaneProber : MonoBehaviour
{
    public int maxNeigborUpdates = 5;
    public float flowStrFalloff = 0.95f;

    public Vector3Int flowFieldResolution = new Vector3Int(1, 1, 1);

	public Vector3[] flowField = null;

	public Transform flowObject = null;
	public float flowObjectRange = 1f;
	public float mul = 1f;
	public float flowSpeedMul = 1f;

	public struct FlowFieldNode
	{
		Vector3 velocity;
		Vector3 worldPos;
	};

	public const int FLOWFIELDSTRIDE = (sizeof(float) * 6);

	[Button]
	private void Start()
	{
		flowField = new Vector3[flowFieldResolution.x * flowFieldResolution.y *  flowFieldResolution.z];

		for (int x = 0; x < flowFieldResolution.x; x++)
		{
			for (int y = 0; y < flowFieldResolution.y; y++)
			{
				for (int z = 0; z < flowFieldResolution.z; z++)
				{
					float _x = Random.value * 2f - 1;
					float _y = Random.value * 2f - 1;
					float _z = Random.value * 2f - 1;
					flowField[GetArrayIndex(x, y, z)] = new Vector3(_x, _y, _z);
				}
			}
		}
	}

	Vector3 sphereVelocity;
	Vector3 prevSpherePos;
	float velocityMagnitude = 1.0f;
	public float turbulenceFactor;

	private void Update()
	{
		for (int x = 0; x < flowFieldResolution.x; x++)
		{
			for (int y = 0; y < flowFieldResolution.y; y++)
			{
				for (int z = 0; z < flowFieldResolution.z; z++)
				{
					Vector3 turbulence = Random.insideUnitSphere;
					Vector3 spherePosition = (transform.position + new Vector3(x, y, z)) - flowObject.position;
					Vector3 sphereVelocity = prevSpherePos - flowObject.position;
					velocityMagnitude = sphereVelocity.magnitude;
					float objDist = Mathf.Max(spherePosition.magnitude, 0.0001f);
					float range = Mathf.Max(flowObjectRange, 0.0001f);

					float factor = Mathf.InverseLerp(0, 3f, velocityMagnitude);

					Vector3 flowDir = Vector3.zero;
					if(objDist < range)
					{
						// 1f when close to center of range, 0f when near edge of range
						float actingNumber = ((1.0f / objDist) / (1.0f / range));
						flowDir = sphereVelocity * actingNumber * factor;
					}


					flowField[GetArrayIndex(x, y, z)] = Vector3.Lerp(flowField[GetArrayIndex(x, y, z)], GetNeighborDir(x, y, z), Mathf.Clamp01(0.15f * flowSpeedMul)) + (flowDir * mul) + (-turbulence * turbulenceFactor * Time.deltaTime);
				}
			}
		}
		prevSpherePos = flowObject.transform.position;
	}

	// current array is constructed z -> y -> x so formula will be (z + YSIZE * y + (x * YSIZE * XSIZE))
	private int GetArrayIndex(int x, int y, int z)
	{
		return z + (flowFieldResolution.y * y) + ((flowFieldResolution.y * flowFieldResolution.x * x));
	}

	private void OnDrawGizmos()
	{
		if (flowField == null)
			return;

		for (int x = 0; x < flowFieldResolution.x; x++)
		{
			for (int y = 0; y < flowFieldResolution.y; y++)
			{
				for (int z = 0; z < flowFieldResolution.z; z++)
				{
					Vector3 pos = transform.position + new Vector3(x, y, z);
					Vector3 dir = pos + flowField[GetArrayIndex(x, y, z)].normalized;
					Gizmos.color = new Color(Mathf.Clamp01(Mathf.Abs(flowField[GetArrayIndex(x, y, z)].x / 2 + 1)), Mathf.Clamp01(Mathf.Abs(flowField[GetArrayIndex(x, y, z)].y / 2 + 1)), Mathf.Clamp01(Mathf.Abs(flowField[GetArrayIndex(x, y, z)].z / 2 + 1)));
					Gizmos.DrawLine(pos, dir);
					if(flowFieldResolution.x * flowFieldResolution.y * flowFieldResolution.z <= 1000f)
						Gizmos.DrawSphere(dir, 0.1f);
				}
			}
		}
	}

	public Vector3 GetNeighborDir(int x, int y, int z)
	{
		Vector3 avgVal = flowField[GetArrayIndex(x, y, z)];

		int countedNeighbors = 1;

		for (int dz = z - 1; dz <= z + 1; ++dz)
		{
			if (dz < 0 || dz >= flowFieldResolution.z)
				continue;
			for (int dy = y - 1; dy <= y + 1; ++dy)
			{
				if (dy < 0 || dy >= flowFieldResolution.y)
					continue;
				for (int dx = x - 1; dx <= x + 1; ++dx)
				{
					// all 27
					if (dx < 0 || dx >= flowFieldResolution.x)
						continue;
					if ((dx != x) || (dy != y) || (dz != z))
					{
						countedNeighbors++;
						avgVal += flowField[GetArrayIndex(dx, dy, dz)];
					}
				}
			}
		}

		avgVal /= countedNeighbors;
		return avgVal;
	}
}
