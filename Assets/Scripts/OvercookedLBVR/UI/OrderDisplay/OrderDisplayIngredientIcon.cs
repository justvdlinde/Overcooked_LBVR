using UnityEngine;
using UnityEngine.UI;

public class OrderDisplayIngredientIcon : MonoBehaviour
{

    [SerializeField] private Image image = null;

    public void SetSprite(Sprite sprite)
    {
        image.sprite = sprite;
    }
}
