using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsCharacter
{
	public class SingleHandlePositionDelegate : ToolPositionDelegate
	{
		[SerializeField] private ToolHandle connectedToolHandle = null;

		public override Vector3 GetPosition()
		{
			return connectedToolHandle.transform.position;
		}

		public override Vector3 GetAnchorPosition()
		{
			return connectedToolHandle.localTransformMirror.GetWorldPosition();
		}

		public override Quaternion GetRotation()
		{
			return connectedToolHandle.transform.rotation;
		}
	}
}