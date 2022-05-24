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

	public bool WasOnFire { get; private set; }

	private const string ShaderRottenVar = "_RottenValue";
	private const string ShaderFrozenVar = "_FrozenValue";
	private const string ShaderWetVar = "_WetValue";
	private const string ShaderOnFireVar = "_OnFireValue";

	[SerializeField] private Ingredient connectedIngredient = null;
	public bool CanCook => currentHeat >= 0f;
	public bool CanChop => currentHeat >= -30f && !IsFrozen;
	public bool CanPickUp => !IsOnFire;

	private Coroutine CountdownCoroutine = null;

	private float maxHeat = 100f;

	public float currentHeat = 0f;

	private float onFireValue = 75f;
	private float wetValue = -30f;
	private float frozenValue = -75f;
	private float minHeat = -100f;

	public ParticleSystem IsRottenParticles = null;
	public ParticleSystem IsWetParticles = null;
	public ParticleSystem IsFrozenParticles = null;
	public ParticleSystem IsOnFireParticles = null;

	public List<Material> shadedMaterials = null;

	private float IsRottenValue = 0;
	private float IsWetValue = 0;
	private float IsFrozenValue = 0;
	private float IsOnFireValue = 0;

	[SerializeField] private GameObject countdownCanvas = null;
	[SerializeField] private TMPro.TextMeshProUGUI countdownText = null;

	[SerializeField] private Color startColor = Color.white;
	
	[SerializeField] private Color endColor = Color.green;
	[SerializeField] private float yOffset = 0.0f;

	private void Awake()
	{
		shadedMaterials = new List<Material>();
		MeshRenderer[] ms = GetComponentsInChildren<MeshRenderer>(true);
		foreach (var item in ms)
		{
			shadedMaterials.Add(item.material);
		}

		yOffset = countdownCanvas.transform.localPosition.y;

		countdownCanvas.SetActive(false);
	}

	private void OnEnable()
	{
		WasOnFire = false;
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
		IsOnFireValue = SetValue(IsOnFire || WasOnFire, IsOnFireValue);

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

		countdownCanvas.transform.position = connectedIngredient.transform.position + Vector3.up * yOffset;
		countdownCanvas.transform.rotation = Quaternion.identity;
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
					item.SetFloat(ShaderOnFireVar, (IsOnFire || WasOnFire) ? 1 : 0);
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
				WasOnFire = true;
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
			IsWetValue = 1f;
			ToggleParticleSystems(true);
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

	public float GetDisplayProgress()
	{
		return Mathf.InverseLerp(25, 175, currentHeat + 100f);
	}

	public void SetIsRotting(bool isRotting)
	{
		if(IsRotten)
			return;

		if (isRotting && !isFloorRotting)
		{
			StopAllCoroutines();
			CountdownCoroutine = StartCoroutine(StartCountdown(5));
		}
		else if (!isRotting)
		{
			StopCoroutine(CountdownCoroutine);
			countdownCanvas.SetActive(false);
			isFloorRotting = false;
		}
	}

	bool isFloorRotting = false;
	private IEnumerator StartCountdown(int seconds)
	{
		isFloorRotting = true;
		countdownCanvas.SetActive(true);
		countdownText.text = 5.ToString();
		countdownText.color = startColor;
		int count = 0;
		while(true)
		{
			yield return new WaitForSeconds(1);
			count++;
			countdownText.text = (5 - count).ToString();
			countdownText.color = Color.Lerp(startColor, endColor, (float)count / (float)seconds);

			if (count >= seconds)
				break;
		}
		countdownCanvas.SetActive(false);

		AddStatusCondition(StatusCondition.Rotten);
		isFloorRotting = false;
	}

	public void CopyValues(IngredientStatusCondition copyFrom)
	{
		if (copyFrom == null)
			return;
		copyFrom.SendValues(out IsRottenValue, out IsWetValue, out IsFrozenValue, out IsOnFireValue, out currentHeat, out bool isRotten, out bool isWet, out bool isFrozen, out bool isOnFire, out bool wasOnFire);
		this.IsFrozen = isFrozen;
		this.IsRotten = isRotten;
		this.IsOnFire = isOnFire;
		this.IsWet = isWet;
		this.WasOnFire = wasOnFire;

		ToggleParticleSystems(true);
	}

	public void SendValues(out float IsRottenValue, out float IsWetValue, out float IsFrozenValue, out float IsOnFireValue, out float currentHeat, out bool IsRotten, out bool IsWet, out bool IsFrozen, out bool IsOnFire, out bool WasOnFire)
	{
		IsRottenValue = this.IsRottenValue;
		IsWetValue = this.IsWetValue;
		IsFrozenValue = this.IsFrozenValue;
		IsOnFireValue = this.IsOnFireValue;
		IsRotten = this.IsRotten;
		IsWet = this.IsWet;
		IsFrozen = this.IsFrozen;
		IsOnFire = this.IsOnFire;
		WasOnFire = this.WasOnFire;
		currentHeat = this.currentHeat;
	}
}
