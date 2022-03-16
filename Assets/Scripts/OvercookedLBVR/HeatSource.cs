using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HeatSource : MonoBehaviour
{
    public float heatStrength = 1;

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.TryGetComponent(out CookComponent cookable))
        {
            cookable.Cook(heatStrength);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.TryGetComponent(out CookComponent cookable))
        {
            cookable.isCooking = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out CookComponent cookable))
        {
            if(cookable.isCooking)
                cookable.isCooking = false;
        }
    }
}
