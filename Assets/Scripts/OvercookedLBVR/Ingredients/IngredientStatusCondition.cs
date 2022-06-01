using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;
using Utils.Core.Attributes;

[Flags]
public enum StatusCondition
{
	None = 0,
	Rotten = 1,
	Wet = 2,
	Frozen = 4,
	OnFire = 8
}

public enum StatusConditionHeatSource
{
	Heat,
	Wet,
	Cold
}

public class IngredientStatusCondition : MonoBehaviourPunCallbacks
{
	[SerializeField] private Ingredient connectedIngredient = null;
	[SerializeField] private IngredientStatusConditionGraphics conditionGraphics = null;

	[field: SerializeField]
	public StatusCondition Condition { get; private set; }
	[field: SerializeField]
	public float CurrentHeat { get; private set; }
	[field: SerializeField]

	public bool WasOnFire { get; private set; }
	public bool IsRotten => Condition.HasFlag(StatusCondition.Rotten);
	public bool IsWet => Condition.HasFlag(StatusCondition.Wet);
	public bool IsFrozen => Condition.HasFlag(StatusCondition.Frozen);
	public bool IsOnFire => Condition.HasFlag(StatusCondition.OnFire);

	public bool CanCook => CurrentHeat >= 0f;
	public bool CanChop => CurrentHeat >= -30f && !IsFrozen;
	public bool CanPickUp => !IsOnFire;

	private const float MaxHeat = 100f;
	private const float OnFireValue = 75f;
	private const float WetValue = -30f;
	private const float FrozenValue = -75f;
	private const float MinHeat = -100f;

	[Header("Debugging")]
	public StatusCondition testCondition = StatusCondition.Frozen;
	public bool testInstantUpdateParticles = false;

	public override void OnPlayerEnteredRoom(PhotonNetworkedPlayer newPlayer)
	{
		if (PhotonNetwork.IsMasterClient)
			SendSyncData(newPlayer);
	}

	private void SendSyncData(PhotonNetworkedPlayer player)
	{
		photonView.RPC(nameof(CopyValuesRPC), player, Condition, CurrentHeat, WasOnFire, conditionGraphics.IsRottenValue, conditionGraphics.IsWetValue, conditionGraphics.IsFrozenValue, conditionGraphics.IsOnFireValue);
	}

	public bool IsIngredientPreparedProperly()
	{
		return !IsOnFire && !IsWet && !IsRotten && !IsFrozen && !WasOnFire;
	}

	public void SetStatusCondition(StatusCondition condition, bool instantUpdateParticles = true)
    {
		photonView.RPC(nameof(SetStatusConditionRPC), RpcTarget.All, condition, instantUpdateParticles);
	}

	[PunRPC]
	private void SetStatusConditionRPC(StatusCondition condition, bool instantUpdateParticles)
	{
		Condition = condition;
		if (photonView.IsMine && instantUpdateParticles)
			conditionGraphics.AddStatusCondition(condition);
	}

	public StatusCondition AddStatusCondition(StatusCondition condition, StatusCondition add)
	{
		return condition |= add;
	}

	public StatusCondition RemoveStatusCondition(StatusCondition condition, StatusCondition remove)
	{
		return condition &= ~remove;
	}

	public void AddHeat(float heatStrength, StatusConditionHeatSource sourceType)
	{
		if (IsOnFire && sourceType == StatusConditionHeatSource.Heat)
			return;
		if (WasOnFire && CurrentHeat >= 0.0 && sourceType == StatusConditionHeatSource.Heat)
			return;
		if (IsWet && sourceType == StatusConditionHeatSource.Wet)
			return;
		if (IsFrozen && sourceType == StatusConditionHeatSource.Cold)
			return;

		float maxHeatLocal = (connectedIngredient.NeedsToBeCooked && connectedIngredient.GetCookState() != CookState.Burned) ? 0.0f : MaxHeat;
		CurrentHeat += heatStrength;
		CurrentHeat = Mathf.Clamp(CurrentHeat, MinHeat, maxHeatLocal);

		if (sourceType == StatusConditionHeatSource.Wet)
		{
			if (IsFrozen)
				return;
			CurrentHeat = WetValue;
			StatusCondition newCondition = Condition;
			newCondition = RemoveStatusCondition(newCondition, StatusCondition.Frozen);
			newCondition = AddStatusCondition(newCondition, StatusCondition.Wet);
			newCondition = RemoveStatusCondition(newCondition, StatusCondition.OnFire);
			SetStatusCondition(newCondition);
		}
		else
		{
			if (IsWet && CurrentHeat >= 0.0f)
			{
				StatusCondition newCondition = Condition;
				newCondition = RemoveStatusCondition(newCondition, StatusCondition.Frozen);
				newCondition = RemoveStatusCondition(newCondition, StatusCondition.Wet);
				newCondition = RemoveStatusCondition(newCondition, StatusCondition.OnFire);
				SetStatusCondition(newCondition);
			}

            if (IsFrozen && CurrentHeat >= 0.0f)
            {
				StatusCondition newCondition = Condition;
				newCondition = RemoveStatusCondition(newCondition, StatusCondition.Frozen);
				newCondition = RemoveStatusCondition(newCondition, StatusCondition.Wet);
				newCondition = RemoveStatusCondition(newCondition, StatusCondition.OnFire);
				SetStatusCondition(newCondition);
            }

            if (IsOnFire && CurrentHeat <= 0.0f)
            {
				StatusCondition newCondition = Condition;
				newCondition = RemoveStatusCondition(newCondition, StatusCondition.Frozen);
				newCondition = RemoveStatusCondition(newCondition, StatusCondition.Wet);
				newCondition = RemoveStatusCondition(newCondition, StatusCondition.OnFire);
				SetStatusCondition(newCondition);
			}

			if (CurrentHeat < FrozenValue)
            {
				StatusCondition newCondition = Condition;
				newCondition = AddStatusCondition(newCondition, StatusCondition.Frozen);
				newCondition = RemoveStatusCondition(newCondition, StatusCondition.Wet);
				newCondition = RemoveStatusCondition(newCondition, StatusCondition.OnFire);
				SetStatusCondition(newCondition);
			}

			if (CurrentHeat > OnFireValue)
            {
				StatusCondition newCondition = Condition;
				newCondition = RemoveStatusCondition(newCondition, StatusCondition.Frozen);
				newCondition = RemoveStatusCondition(newCondition, StatusCondition.Wet);
				newCondition = AddStatusCondition(newCondition, StatusCondition.OnFire);
				SetStatusCondition(newCondition);
			}
		}
	}

	public void CopyValues(IngredientStatusCondition copyFrom)
	{
		if (copyFrom == null)
			return;

		copyFrom.SendValues(out StatusCondition condition, out float currentHeat, out bool wasOnFire, out float rottenValue, out float wetValue, out float frozenValue, out float onFireValue);
		photonView.RPC(nameof(CopyValuesRPC), RpcTarget.All, condition, currentHeat, wasOnFire, rottenValue, wetValue, frozenValue, onFireValue);
	}

	[PunRPC]
	public void CopyValuesRPC(StatusCondition condition, float currentHeat, bool wasOnFire, float rottenValue, float wetValue, float frozenValue, float onFireValue)
	{
		conditionGraphics.SendValues(rottenValue, wetValue, frozenValue, onFireValue);
		Condition = condition;
		WasOnFire = wasOnFire;
		CurrentHeat = currentHeat;
	}

	// read values from parent status condition when being cut into 2 parts
	public void SendValues(out StatusCondition condition, out float currentHeat, out bool wasOnFire, out float rottenValue, out float wetValue, out float frozenValue, out float onFireValue)
	{
		condition = Condition;
		currentHeat = CurrentHeat;
		wasOnFire = WasOnFire;

		rottenValue = conditionGraphics.IsRottenValue;
		wetValue = conditionGraphics.IsWetValue;
		frozenValue = conditionGraphics.IsFrozenValue;
		onFireValue = conditionGraphics.IsOnFireValue;
	}

#if UNITY_EDITOR
	[Button]
	public void TestAddCondition()
	{
		SetStatusCondition(AddStatusCondition(Condition, testCondition));
	}

	[Button]
	public void TestRemoveCondition()
	{
		SetStatusCondition(RemoveStatusCondition(Condition, testCondition));
	}
#endif
}
