using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SnapPoint : MonoBehaviour
{
    public bool IsFilled => isFilled;
    [SerializeField] private bool isFilled = false; 

    private void OnValidate()
    {
        if (TryGetComponent(out Collider collider))
            collider.isTrigger = true;
    }

    public void Snap(GameObject other)
    {
        Debug.Log("snapping " + other.gameObject.name + " to " + gameObject.name);

        // TODO: test with (fast) lerp
        other.transform.SetParent(transform);
        other.transform.localPosition = Vector3.zero;
        other.transform.localEulerAngles = new Vector3(0, other.transform.eulerAngles.y, 0);

        if (other.TryGetComponent(out Rigidbody rigidbody))
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
        }

        isFilled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Ingredient item))
        {
            Snap(item.gameObject);
        }
    }

    public bool CanBeSnappedTo()
    {
        return isFilled == false;
    }
}
