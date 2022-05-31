using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.Core.Attributes;

public enum StatusCondition
{
	Rotten,
	Wet,
	Frozen,
	OnFire
}

public enum StatusConditionHeatSource
{
	Heat,
	Wet,
	Cold
}
public class IngredientStatusCondition : MonoBehaviour
{

	[field: SerializeField]
	public bool IsRotten { get; private set; }
	[field: SerializeField]
	public bool IsWet { get; private set; }
	[field: SerializeField]
	public bool IsFrozen { get; private set; }
	[field: SerializeField]
	public bool IsOnFire { get; private set; }

	public bool WasOnFire { get; private set; }


	[SerializeField] private Ingredient connectedIngredient = null;
	public bool CanCook => currentHeat >= 0f;
	public bool CanChop => currentHeat >= -30f && !IsFrozen;
	public bool CanPickUp => !IsOnFire;

	public float CurrentHeat => currentHeat;

	private float maxHeat = 100f;
	public float currentHeat = 0f;
	private float onFireValue = 75f;
	private float wetValue = -30f;
	private float frozenValue = -75f;
	private float minHeat = -100f;

	[SerializeField] private IngredientStatusConditionGraphics conditionGraphics = null;

	[SerializeField] private PhotonView photonView = null;

	public bool IsIngredientPreparedProperly()
	{
		return !IsOnFire && !IsWet && !IsRotten && !IsFrozen && !WasOnFire;
	}

	public StatusCondition c = StatusCondition.Frozen;
	public bool updateP = false;

	[Button]
	public void TestAddCondition()
	{
		AddStatusCondition(c, updateP);
	}

	[Button]
	public void TestRemoveCondition()
	{
		RemoveStatusCondition(c, updateP);
	}

	public void AddStatusCondition(StatusCondition condition, bool instantUpdateParticles = true)
	{
		photonView.RPC(nameof(AddStatusConditionRPC), RpcTarget.All, condition, instantUpdateParticles);
	}

	[PunRPC]
	private void AddStatusConditionRPC(StatusCondition condition, bool instantUpdateParticles)
	{
		switch (condition)
		{
			case StatusCondition.Rotten:
				IsRotten = true;
				break;
			case StatusCondition.Wet:
				IsWet = true;
				break;
			case StatusCondition.Frozen:
				IsFrozen = true;
				break;
			case StatusCondition.OnFire:
				IsOnFire = true;
				WasOnFire = true;
				break;
			default:
				break;
		}
		if (photonView.IsMine && instantUpdateParticles)
			conditionGraphics.AddStatusCondition(condition);
	}

	public void RemoveStatusCondition(StatusCondition condition, bool instantUpdateParticles = true)
	{
		photonView.RPC(nameof(RemoveStatusConditionRPC), RpcTarget.All, condition, instantUpdateParticles);
	}

	[PunRPC]
	private void RemoveStatusConditionRPC(StatusCondition condition, bool instantUpdateParticles)
	{
		switch (condition)
		{
			case StatusCondition.Rotten:
				IsRotten = false;
				break;
			case StatusCondition.Wet:
				IsWet = false;
				break;
			case StatusCondition.Frozen:
				IsFrozen = false;
				break;
			case StatusCondition.OnFire:
				IsOnFire = false;
				break;
			default:
				break;
		}
		if(photonView.IsMine && instantUpdateParticles)
			conditionGraphics.RemoveStatusCondition(condition);
	}

	public void AddHeat(float heatStrength, StatusConditionHeatSource sourceType)
	{
		if (IsOnFire && sourceType == StatusConditionHeatSource.Heat)
			return;
		if (WasOnFire && currentHeat >= 0.0 && sourceType == StatusConditionHeatSource.Heat)
			return;
		if (IsWet && sourceType == StatusConditionHeatSource.Wet)
			return;
		if (IsFrozen && sourceType == StatusConditionHeatSource.Cold)
			return;

		float maxHeatLocal = (connectedIngredient.NeedsToBeCooked && connectedIngredient.GetCookState() != CookState.Burned) ? 0.0f : maxHeat;
		currentHeat += heatStrength;
		currentHeat = Mathf.Clamp(currentHeat, minHeat, maxHeatLocal);


		if (sourceType == StatusConditionHeatSource.Wet)
		{
			if (IsFrozen)
				return;
			currentHeat = -30f;
			RemoveStatusCondition(StatusCondition.Frozen, false);
			// force set particles here
			AddStatusCondition(StatusCondition.Wet, true);
			RemoveStatusCondition(StatusCondition.OnFire, false);
		}
		else
		{
			if (IsWet && currentHeat >= 0.0f)
			{
				RemoveStatusCondition(StatusCondition.Frozen, false);
				RemoveStatusCondition(StatusCondition.Wet, false);
				RemoveStatusCondition(StatusCondition.OnFire, false);
			}

			if (IsFrozen && currentHeat >= 0.0f)
			{
				RemoveStatusCondition(StatusCondition.Frozen, false);
				RemoveStatusCondition(StatusCondition.Wet, false);
				RemoveStatusCondition(StatusCondition.OnFire, false);
			}

			if (IsOnFire && currentHeat <= 0.0f)
			{
				RemoveStatusCondition(StatusCondition.Frozen, false);
				RemoveStatusCondition(StatusCondition.Wet, false);
				RemoveStatusCondition(StatusCondition.OnFire, false);
			}

			if (currentHeat < frozenValue)
			{
				AddStatusCondition(StatusCondition.Frozen, false);
				RemoveStatusCondition(StatusCondition.Wet, false);
				RemoveStatusCondition(StatusCondition.OnFire, false);
			}
			if (currentHeat > onFireValue)
			{
				RemoveStatusCondition(StatusCondition.Frozen, false);
				RemoveStatusCondition(StatusCondition.Wet, false);
				AddStatusCondition(StatusCondition.OnFire, false);
			}
		}
	}

	public void CopyValues(IngredientStatusCondition copyFrom)
	{
		if (copyFrom == null)
			return;
		copyFrom.SendValues(out float IsRottenValue, out float IsWetValue, out float IsFrozenValue, out float IsOnFireValue, out currentHeat, out bool isRotten, out bool isWet, out bool isFrozen, out bool isOnFire, out bool wasOnFire);

		photonView.RPC(nameof(SetCopyValuesRPC), RpcTarget.All, IsRottenValue, IsWetValue, IsFrozenValue, IsOnFireValue, currentHeat, isRotten, isWet, isFrozen, isOnFire, wasOnFire);
	}

	[PunRPC]
	public void SetCopyValuesRPC(float IsRottenValue, float IsWetValue, float IsFrozenValue, float IsOnFireValue, float currentHeat, bool isRotten, bool isWet, bool isFrozen, bool isOnFire, bool wasOnFire)
	{
		conditionGraphics.SendValues(IsRottenValue, IsWetValue, IsFrozenValue, IsOnFireValue);
		this.IsFrozen = isFrozen;
		this.IsRotten = isRotten;
		this.IsOnFire = isOnFire;
		this.IsWet = isWet;
		this.WasOnFire = wasOnFire;
	}

	// read values from parent status condition when being cut into 2 parts
	public void SendValues(out float IsRottenValue, out float IsWetValue, out float IsFrozenValue, out float IsOnFireValue, out float currentHeat, out bool IsRotten, out bool IsWet, out bool IsFrozen, out bool IsOnFire, out bool WasOnFire)
	{
		IsRottenValue = conditionGraphics .IsRottenValue;
		IsWetValue = conditionGraphics.IsWetValue;
		IsFrozenValue = conditionGraphics.IsFrozenValue;
		IsOnFireValue = conditionGraphics.IsOnFireValue;
		IsRotten = this.IsRotten;
		IsWet = this.IsWet;
		IsFrozen = this.IsFrozen;
		IsOnFire = this.IsOnFire;
		WasOnFire = this.WasOnFire;
		currentHeat = this.currentHeat;
	}
}
