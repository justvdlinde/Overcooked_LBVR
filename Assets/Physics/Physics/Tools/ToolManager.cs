using PhysicsCharacter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolManager : MonoBehaviour
{
	[SerializeField] private List<Tool> tools = new List<Tool>();

	public bool isHoldingTool = false;
	public Tool heldTool = null;
	public int heldToolID = -1;

	private Tool previousHeldtool = null;

	private void Start()
	{
		foreach(Tool t in tools)
		{
			t.gameObject.SetActive(false);
		}

		PhysicsPlayerBlackboard.Instance.snapTurnEvent += DoSnapTurn;
	}

	public void DoSnapTurn(float f)
	{
		PhysicsPlayerBlackboard.Instance.leftController.RotateAround(PhysicsPlayerBlackboard.Instance.headAnchor.position, Vector3.up, f);
		PhysicsPlayerBlackboard.Instance.rightController.RotateAround(PhysicsPlayerBlackboard.Instance.headAnchor.position, Vector3.up, f);
		if(heldTool == null)
			return;

		if(heldTool.IsBeingHeld())
			heldTool.transform.RotateAround(PhysicsPlayerBlackboard.Instance.headAnchor.position, Vector3.up, f);
	}

	public void DoTeleport(Vector3 pos, Vector3 lookRot)
	{
		// needs some calculation for position offset

		PhysicsPlayerBlackboard.Instance.leftController.position += pos;
		PhysicsPlayerBlackboard.Instance.rightController.position += pos;
		if(heldTool == null)
			return;

		if(heldTool.IsBeingHeld())
			heldTool.transform.position += pos;
	}

	public void EquipTool(int i, Hand hand)
	{
		if(isHoldingTool && heldToolID > -1 && heldTool != null)
		{
			UnequipTool(heldToolID, hand);
		}

		// needs to force unequip/equip in physics pickup manager aswell

		// needs to properly reset all pickup items and states on picking up new tool
		// needs option for allowing tools to be within a certain range
		// needs option to let grip go on tool pickup

		tools[i].PickupTool(hand);

		heldTool = tools[i];
		heldToolID = i;
		isHoldingTool = true;
	}

	public void UnequipTool(int i, Hand hand)
	{
		if(i < 0 || i > tools.Count - 1)
			return;

		// needs to force unequip/equip in physics pickup manager aswell
		previousHeldtool = heldTool;

		tools[i].ReleaseTool(hand);
		heldTool = null;
		heldToolID = -1;
		isHoldingTool = false;


		HandsDataDelegate.GetHandedInstance(hand).ResetHandPose();
	}
}
