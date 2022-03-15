using System;
using TMPro;
using UnityEngine;

public class PlayerHealthUI : MonoBehaviour
{
    [SerializeField] protected HealthController healthController = null;
    [SerializeField] protected TextMeshProUGUI hpLabel = null;

    protected virtual void OnEnable()
    {
        healthController.HealthChangedEvent += OnHealthChangedEvent;
    }

    protected virtual void OnDisable()
    {
        healthController.HealthChangedEvent += OnHealthChangedEvent;
    }

    private void OnHealthChangedEvent(float prevHealth, float newHealth)
    {
        hpLabel.text = newHealth.ToString();
    }
}
