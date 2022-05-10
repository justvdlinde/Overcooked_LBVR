using System;
using UnityEngine;
using UnityEngine.UI;

public class SauceUI : MonoBehaviour
{
    [SerializeField] private SauceRecipient sauce = null;
    [SerializeField] private GameObject root = null;
    [SerializeField] private Slider slider = null;
    [SerializeField] private Image sliderImage = null;

    [Header("Colors")]
    [SerializeField] private Color ketchupColor = Color.red;
    [SerializeField] private Color mayoColor = Color.white;

    private void OnEnable()
    {
        sauce.SauceTypeChanged += OnSauceTypeChangedEvent;
    }

    private void OnDisable()
    {
        sauce.SauceTypeChanged += OnSauceTypeChangedEvent;
    }

    private void OnSauceTypeChangedEvent(SauceType sauce)
    {
        sliderImage.color = (sauce == SauceType.Ketchup) ? ketchupColor : mayoColor;
    }

    private void Update()
    {
        root.SetActive(sauce.FillProgressNormalized > 0 && sauce.FoodStack.CanPlaceSauce());
        slider.value = sauce.FillProgressNormalized;
    }
}
