using UnityEngine;
using Utils.Core.Attributes;

public class Dispenser : MonoBehaviour
{
    [SerializeField] private Rigidbody prefab = null;
    [SerializeField] private Transform spawnPoint = null;
    [SerializeField] private float forceAmount = 1;
    [SerializeField] private ForceMode forceMode = ForceMode.Force;

    public void DispenseObject(Rigidbody obj)
    {
        Debug.Log("Dispense");
        Rigidbody instance = Instantiate(obj, spawnPoint.position, spawnPoint.rotation);
        instance.AddForce(instance.transform.forward * forceAmount, forceMode);
    }

    [Button]
    public void DispenseObject()
    {
        DispenseObject(prefab);
    }
}
