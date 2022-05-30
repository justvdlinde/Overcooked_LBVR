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

	public void AddStatusCondition(StatusCondition condition)
	{
		photonView.RPC(nameof(AddStatusConditionRPC), RpcTarget.All, condition);
	}

	[PunRPC]
	private void AddStatusConditionRPC(StatusCondition condition)
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
		if (photonView.IsMine)
			conditionGraphics.AddStatusCondition(condition);
	}

	public void RemoveStatusCondition(StatusCondition condition)
	{
		photonView.RPC(nameof(RemoveStatusConditionRPC), RpcTarget.All, condition);
	}

	[PunRPC]
	private void RemoveStatusConditionRPC(StatusCondition condition)
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
		if(photonView.IsMine)
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
			IsFrozen = false;
			IsOnFire = false;
			IsWet = true;
			conditionGraphics.SetIsWetValue(1f);
		}
		else
		{
			if (IsWet && currentHeat >= 0.0f)
			{
				IsFrozen = false;
				IsWet = false;
				IsOnFire = false;
			}

			if (IsFrozen && currentHeat >= 0.0f)
			{
				IsFrozen = false;
				IsWet = false;
				IsOnFire = false;
			}

			if (IsOnFire && currentHeat <= 0.0f)
			{
				IsFrozen = false;
				IsWet = false;
				IsOnFire = false;
			}

			if (currentHeat < frozenValue)
			{
				IsFrozen = true;
				IsWet = false;
				IsOnFire = false;
			}
			if (currentHeat > onFireValue)
			{
				IsFrozen = false;
				IsWet = false;
				IsOnFire = true;
				WasOnFire = true;
			}
		}
	}

	public void CopyValues(IngredientStatusCondition copyFrom)
	{
		if (copyFrom == null)
			return;
		copyFrom.SendValues(out float IsRottenValue, out float IsWetValue, out float IsFrozenValue, out float IsOnFireValue, out currentHeat, out bool isRotten, out bool isWet, out bool isFrozen, out bool isOnFire, out bool wasOnFire);

		conditionGraphics.SendValues(IsRottenValue, IsWetValue, IsFrozenValue, IsOnFireValue);
		this.IsFrozen = isFrozen;
		this.IsRotten = isRotten;
		this.IsOnFire = isOnFire;
		this.IsWet = isWet;
		this.WasOnFire = wasOnFire;
	}

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
