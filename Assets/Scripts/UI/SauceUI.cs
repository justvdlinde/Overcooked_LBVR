using UnityEngine;
using UnityEngine.UI;

public class SauceUI : MonoBehaviour
{
    [SerializeField] private SauceRecipient sauce = null;
    [SerializeField] private Slider slider = null;
    [SerializeField] private GameObject root = null;

    // TODO: change color depending on sauceType
    private void Update()
    {
        root.SetActive(sauce.FillProgressNormalized > 0);
        slider.value = sauce.FillProgressNormalized;
    }
}
