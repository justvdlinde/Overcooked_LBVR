using UnityEngine;
using UnityEngine.UI;

public class OrderDisplayIngredientIcon : MonoBehaviour
{

    [SerializeField] private Image image = null;

    [SerializeField] private GameObject friedIcon = null;

    public void SetSprite(Sprite sprite)
    {
        image.sprite = sprite;
    }

    public void SetIsFried()
	{
        friedIcon.SetActive(true);
	}
}
