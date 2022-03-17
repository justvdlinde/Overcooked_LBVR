using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsCharacter
{
	// works as a superclass for the single handed and multihanded tools
	public abstract class ToolPositionDelegate : MonoBehaviour
	{
		protected Vector3 position = Vector3.zero;
		protected Quaternion rotation = Quaternion.identity;

		public abstract Vector3 GetPosition();
		public abstract Vector3 GetAnchorPosition();
		public abstract Quaternion GetRotation();
	}
}
