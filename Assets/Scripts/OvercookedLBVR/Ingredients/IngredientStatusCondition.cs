using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.Core.Attributes;

public class IngredientStatusCondition : MonoBehaviour
{
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

	[field: SerializeField]
	public bool IsRotten { get; private set; }
	[field: SerializeField]
	public bool IsWet { get; private set; }
	[field: SerializeField]
	public bool IsFrozen { get; private set; }
	[field: SerializeField]
	public bool IsOnFire { get; private set; }

	private const string ShaderRottenVar = "_RottenValue";
	private const string ShaderFrozenVar = "_FrozenValue";
	private const string ShaderWetVar = "_WetValue";
	private const string ShaderOnFireVar = "_OnFireValue";

	[SerializeField] private Ingredient connectedIngredient = null;
	public bool CanCook => currentHeat >= 0f;

	private float maxHeat = 100f;

	public float currentHeat = 0f;

	private float onFireValue = 75f;
	private float wetValue = -30f;
	private float frozenValue = -75f;
	private float minHeat = -100f;

	public float heatAmt = 10f;
	public StatusConditionHeatSource heatsource = StatusConditionHeatSource.Heat;

	[Button]
	public void AddHeatDebug()
	{
		AddHeat(heatAmt, heatsource);
	}

	public void AddHeat(float heatStrength, StatusConditionHeatSource sourceType)
	{
		if (sourceType == StatusConditionHeatSource.Heat && IsOnFire)
			return;
		if (sourceType == StatusConditionHeatSource.Cold && IsFrozen)
			return;
		if (sourceType == StatusConditionHeatSource.Wet && IsWet)
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
			IsWetValue = 1f;
			ToggleParticleSystems(true);
		}
		else
		{
			if(IsWet && currentHeat >= 0.0f)
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
			}
		}

	}



	public ParticleSystem IsRottenParticles = null;
	public ParticleSystem IsWetParticles = null;
	public ParticleSystem IsFrozenParticles = null;
	public ParticleSystem IsOnFireParticles = null;

	public List<Material> shadedMaterials = null;

	private float IsRottenValue = 0;
	private float IsWetValue = 0;
	private float IsFrozenValue = 0;
	private float IsOnFireValue = 0;

	private void Awake()
	{
		shadedMaterials = new List<Material>();
		MeshRenderer[] ms = GetComponentsInChildren<MeshRenderer>(true);
		foreach (var item in ms)
		{
			shadedMaterials.Add(item.material);
		}
	}

	private void OnEnable()
	{
		if (shadedMaterials != null && shadedMaterials.Count > 0)
		{
			shadedMaterials = new List<Material>();
			MeshRenderer[] ms = GetComponentsInChildren<MeshRenderer>(true);
			foreach (var item in ms)
			{
				shadedMaterials.Add(item.material);
			}
		}

		IsRottenValue = (IsRotten) ? 1 : 0;
		IsWetValue = (IsWet) ? 1 : 0;
		IsFrozenValue = (IsFrozen) ? 1 : 0;
		IsOnFireValue = (IsOnFire) ? 1 : 0;

		ToggleParticleSystems();
	}

	private void Update()
	{
		IsRottenValue = SetValue(IsRotten, IsRottenValue);
		IsWetValue = SetValue(IsWet, IsWetValue);
		IsFrozenValue = SetValue(IsFrozen, IsFrozenValue);
		IsOnFireValue = SetValue(IsOnFire, IsOnFireValue);

		if (shadedMaterials != null)
		{
			foreach (var item in shadedMaterials)
			{
				item.SetFloat(ShaderRottenVar, IsRottenValue);
				item.SetFloat(ShaderWetVar, IsWetValue);
				item.SetFloat(ShaderFrozenVar, IsFrozenValue);
				item.SetFloat(ShaderOnFireVar, IsOnFireValue);
			}
		}

		ToggleParticleSystems();
	}

	private float SetValue(bool Condition, float Value)
	{
		float returnVal = Value;
		if (Condition)
		{
			if (returnVal < 1)
				returnVal += Time.deltaTime;
			else
				returnVal = 1;
		}
		else
		{
			if (returnVal > 0)
				returnVal -= Time.deltaTime;
			else
				returnVal = 0;
		}

		return returnVal;
	}

	[Button]
	private void ToggleParticleSystems(bool SetShaderVals = false)
	{
		IsRottenParticles.gameObject.SetActive(IsRotten && IsRottenValue >= 1f);
		IsWetParticles.gameObject.SetActive(IsWet && IsWetValue >= 1f);
		IsFrozenParticles.gameObject.SetActive(IsFrozen && IsFrozenValue >= 1f);
		IsOnFireParticles.gameObject.SetActive(IsOnFire && IsOnFireValue >= 1f);

		if (SetShaderVals)
		{
			if (shadedMaterials != null)
			{
				foreach (var item in shadedMaterials)
				{
					item.SetFloat(ShaderRottenVar, (IsRotten) ? 1 : 0);
					item.SetFloat(ShaderWetVar, (IsWet) ? 1 : 0);
					item.SetFloat(ShaderFrozenVar, (IsFrozen) ? 1 : 0);
					item.SetFloat(ShaderOnFireVar, (IsOnFire) ? 1 : 0);
				}
			}
		}
	}

	public void AddStatusCondition(StatusCondition condition)
	{
		switch (condition)
		{
			case StatusCondition.Rotten:
				IsRotten = true;
				IsRottenValue = 1;
				break;
			case StatusCondition.Wet:
				IsWet = true;
				IsWetValue = 1;
				break;
			case StatusCondition.Frozen:
				IsFrozen = true;
				IsFrozenValue = 1;
				break;
			case StatusCondition.OnFire:
				IsOnFire = true;
				IsOnFireValue = 1;
				break;
			default:
				break;
		}
		ToggleParticleSystems(true);
	}

	public void RemoveStatusCondition(StatusCondition condition)
	{
		switch (condition)
		{
			case StatusCondition.Rotten:
				IsRotten = false;
				IsRottenValue = 0;
				break;
			case StatusCondition.Wet:
				IsWet = false;
				IsWetValue = 0;
				break;
			case StatusCondition.Frozen:
				IsFrozen = false;
				IsFrozenValue = 0;
				break;
			case StatusCondition.OnFire:
				IsOnFire = false;
				IsOnFireValue = 0;
				break;
			default:
				break;
		}
		ToggleParticleSystems(true);
	}
}
