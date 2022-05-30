using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngredientStatusConditionGraphics : MonoBehaviour, IRottable
{
	private const string ShaderRottenVar = "_RottenValue";
	private const string ShaderFrozenVar = "_FrozenValue";
	private const string ShaderWetVar = "_WetValue";
	private const string ShaderOnFireVar = "_OnFireValue";
	public ParticleSystem IsRottenParticles = null;
	public ParticleSystem IsWetParticles = null;
	public ParticleSystem IsFrozenParticles = null;
	public ParticleSystem IsOnFireParticles = null;
	public List<Material> shadedMaterials = null;
	[SerializeField] private GameObject countdownCanvas = null;
	[SerializeField] private TMPro.TextMeshProUGUI countdownText = null;

	[SerializeField] private Color startColor = Color.white;

	[SerializeField] private Color endColor = Color.green;
	[SerializeField] private float yOffset = 0.0f;

	public float IsRottenValue { get; private set; } = 0;
	public float IsWetValue { get; private set; } = 0;
	public float IsFrozenValue { get; private set; } = 0;
	public float IsOnFireValue { get; private set; } = 0;

	private Coroutine CountdownCoroutine = null;
	private bool isFloorRotting = false;

	[SerializeField] private Ingredient connectedIngredient = null;
	[SerializeField] private IngredientStatusCondition connectedCondition = null;

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
		if (shadedMaterials != null && shadedMaterials.Count > 0)
		{
			shadedMaterials = new List<Material>();
			MeshRenderer[] ms = GetComponentsInChildren<MeshRenderer>(true);
			foreach (var item in ms)
			{
				shadedMaterials.Add(item.material);
			}
		}

		IsRottenValue = (connectedCondition.IsRotten) ? 1 : 0;
		IsWetValue = (connectedCondition.IsWet) ? 1 : 0;
		IsFrozenValue = (connectedCondition.IsFrozen) ? 1 : 0;
		IsOnFireValue = (connectedCondition.IsOnFire) ? 1 : 0;

		ToggleParticleSystems();
	}

	private void Update()
	{
		IsRottenValue = SetValue(connectedCondition.IsRotten, IsRottenValue);
		IsWetValue = SetValue(connectedCondition.IsWet, IsWetValue);
		IsFrozenValue = SetValue(connectedCondition.IsFrozen, IsFrozenValue);
		IsOnFireValue = SetValue(connectedCondition.IsOnFire || connectedCondition.WasOnFire, IsOnFireValue);

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

	private void ToggleParticleSystems(bool SetShaderVals = false)
	{
		IsRottenParticles.gameObject.SetActive(connectedCondition.IsRotten && IsRottenValue >= 1f);
		IsWetParticles.gameObject.SetActive(connectedCondition.IsWet && IsWetValue >= 1f);
		IsFrozenParticles.gameObject.SetActive(connectedCondition.IsFrozen && IsFrozenValue >= 1f);
		IsOnFireParticles.gameObject.SetActive(connectedCondition.IsOnFire && IsOnFireValue >= 1f);

		if (SetShaderVals)
		{
			if (shadedMaterials != null)
			{
				foreach (var item in shadedMaterials)
				{
					item.SetFloat(ShaderRottenVar, (connectedCondition.IsRotten) ? 1 : 0);
					item.SetFloat(ShaderWetVar, (connectedCondition.IsWet) ? 1 : 0);
					item.SetFloat(ShaderFrozenVar, (connectedCondition.IsFrozen) ? 1 : 0);
					item.SetFloat(ShaderOnFireVar, (connectedCondition.IsOnFire || connectedCondition.WasOnFire) ? 1 : 0);
				}
			}
		}
	}

	public float GetDisplayProgress()
	{
		return Mathf.InverseLerp(25, 175, connectedCondition.CurrentHeat + 100f);
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

	public void SetIsRotting(bool isRotting)
	{
		if (connectedCondition.IsRotten)
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

	internal void SetIsWetValue(float v)
	{
		IsWetValue = v;
	}

	private IEnumerator StartCountdown(int seconds)
	{
		isFloorRotting = true;
		countdownCanvas.SetActive(true);
		countdownText.text = 5.ToString();
		countdownText.color = startColor;
		int count = 0;
		while (true)
		{
			yield return new WaitForSeconds(1);
			count++;
			countdownText.text = (5 - count).ToString();
			countdownText.color = Color.Lerp(startColor, endColor, (float)count / (float)seconds);

			if (count >= seconds)
				break;
		}
		countdownCanvas.SetActive(false);

		connectedCondition.AddStatusCondition(StatusCondition.Rotten);
		isFloorRotting = false;
	}

	public void SendValues(float isRottenValue, float isWetValue, float isFrozenValue, float isOnFireValue)
	{
		IsRottenValue = isRottenValue;
		IsWetValue = isWetValue;
		IsFrozenValue = isFrozenValue;
		IsOnFireValue = isOnFireValue;
	}

	public void AddStatusCondition(StatusCondition condition)
	{
		switch (condition)
		{
			case StatusCondition.Rotten:
				IsRottenValue = 1;
				break;
			case StatusCondition.Wet:
				IsWetValue = 1;
				break;
			case StatusCondition.Frozen:
				IsFrozenValue = 1;
				break;
			case StatusCondition.OnFire:
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
				IsRottenValue = 0;
				break;
			case StatusCondition.Wet:
				IsWetValue = 0;
				break;
			case StatusCondition.Frozen:
				IsFrozenValue = 0;
				break;
			case StatusCondition.OnFire:
				IsOnFireValue = 0;
				break;
			default:
				break;
		}
		ToggleParticleSystems(true);
	}
}
