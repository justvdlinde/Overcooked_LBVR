using Photon.Pun;
using UnityEditor;
using UnityEngine;

public class CalibrationCursor : MonoBehaviour
{
	public static CalibrationCursor Instance = null;
	
	[SerializeField] private Transform cursor = null;

	private Vector3 rawPos = new Vector3(0.5f, 0f, 0.5f);
	private Vector3 rawRot = new Vector3(0.5f, 0f, 0.55f);

	public Vector2 GetDimensions(bool isMeters = true)
	{
		if (isMeters)
			return new Vector2(transform.localScale.x, transform.localScale.z);
		else
			return new Vector2(transform.localScale.x, transform.localScale.z) * 3.28f;
	}

	private void Awake()
	{
		Instance = this;

		// Commented because throws error when switching scenes via network
		//if (!PhotonNetwork.IsMasterClient)
		//{
		//	PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(RoomPropertiesPhoton.CALIBRATION_POSITION_X, out object x);
		//	PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(RoomPropertiesPhoton.CALIBRATION_POSITION_Z, out object z);
		//	Debug.Log("x " + x);
		//	Debug.Log("z " + z);
		//	float xPos = (float)x;// (x != null) ? int.Parse(x as string) : 0;
		//	float zPos = (float)z;// (z != null) ? int.Parse(z as string) : 0;
		//	SetCursorPosition(new Vector3(xPos, 0, zPos));
		//}
	}

	public void SetCursorPosition(Vector3 position)
	{
		Vector3 pos = position;
		pos.x = Mathf.Clamp01(pos.x);
		pos.z = Mathf.Clamp01(pos.z);
		pos.y = 0f;
		rawPos = pos;
		cursor.localPosition = pos;
	}

	public void SetCursorRotation(Vector3 rotation)
	{
		Vector3 v = rotation;
		v.x = Mathf.Clamp01(v.x);
		v.z = Mathf.Clamp01(v.z);
		v.y = 0f;
		rawRot = v;

		Vector3 rawDir = (rawRot - rawPos);
		cursor.eulerAngles = new Vector3(0, Mathf.Atan2(rawDir.x, rawDir.z) * Mathf.Rad2Deg, 0);
	}

	public Vector3 GetCalibrationCursorPosition()
	{
		//if(PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(RoomPropertiesPhoton.CALIBRATION_POSITION_X, out object xPos) && PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(RoomPropertiesPhoton.CALIBRATION_POSITION_Z, out object zPos))
		//{
		//	Vector3 newPos = new Vector3((float)xPos, 0.0f, (float)zPos);
		//	newPos = cursor.TransformPoint(newPos);
		//	Debug.Log("Getting calib pos from room properties: " + newPos);
		//	return newPos;
		//}
		//else
		//{
		return cursor.position;
		//}
	}

	public Vector3 GetCalibrationCursorLocalPosition()
	{
		return cursor.localPosition;
	}

	public Vector3 GetCalibrationCursorRotation()
	{
		return cursor.forward;
	}

	public Quaternion GetCursorRotation()
	{
		return cursor.rotation;
	}

	private void OnDrawGizmosSelected()
	{
#if UNITY_EDITOR
		if (cursor != null)
		{
			if (Selection.activeGameObject == transform.gameObject)
			{
				Color c = Color.red;
				c.a = 0.5f;
				Gizmos.color = c;
				Gizmos.DrawCube(transform.position + transform.lossyScale * 0.5f, transform.lossyScale);
				c = Color.cyan;
				Gizmos.color = c;
				Gizmos.DrawCube(cursor.position, new Vector3(0.3f, 10f, 0.3f));

				c = Color.yellow;
				Gizmos.color = c;
				//Gizmos.DrawCube(cursor2.position, new Vector3(0.3f, 10f, 0.3f));

				Gizmos.DrawCube(cursor.position + cursor.forward * 0.3f, new Vector3(0.3f, 10f, 0.3f));
			}
		}
#endif
	}
}
