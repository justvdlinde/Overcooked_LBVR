using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowWallScript : MonoBehaviour
{
    public float travelTime;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
        
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        rb.velocity /= travelTime;
        Debug.Log(other.name);

    
    
    }

    private void OnTriggerExit(Collider other)
    {

    }

}
