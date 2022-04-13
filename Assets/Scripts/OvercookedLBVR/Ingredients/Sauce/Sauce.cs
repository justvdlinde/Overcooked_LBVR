using UnityEngine;

public class Sauce : MonoBehaviour
{
    public float FillAmount { get; private set; }
    public bool IsFullyPlaced => FillAmount >= 100;

    [SerializeField] private float fillSpeed = 1;

    public void Fill(float amount)
    {
        FillAmount = amount * fillSpeed * Time.deltaTime;
        FillAmount = Mathf.Clamp(0, 100, FillAmount);
    }

    // TODO: scale graphics to fillAmount
}
