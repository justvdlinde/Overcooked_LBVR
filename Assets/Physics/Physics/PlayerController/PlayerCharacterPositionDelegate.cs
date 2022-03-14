using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsCharacter
{
	public class PlayerCharacterPositionDelegate : MonoBehaviour
	{
		public static PlayerCharacterPositionDelegate Instance = null;

		private Transform followTarget = null;

		private void Awake()
		{
			if(Instance == null)
				Instance = this;
			else
				this.enabled = false;
		}

		public Vector3 GestFollowPosition()
		{
			return followTarget.position;
		}

		public Quaternion GetFollowRotation()
		{
			return followTarget.rotation;
		}

		public void SetNewFollowTarget(Transform t)
		{
			followTarget = t;
		}

		public void ResetFollowTarget()
		{
			followTarget = null;
		}

		public bool HasFollowTarget()
		{
			return followTarget != null;
		}
	}
}