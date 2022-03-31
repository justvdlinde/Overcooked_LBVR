using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerScript : MonoBehaviour
{

    public GameObject wallPointer;
    public GameObject pointer;
    public GameObject target;
    public GameObject origin;

    public Vector3 originPosition;
    public Vector3 targetPosition;
    public Vector3 wallPosition;

    public Vector3 LookDirection;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        originPosition = origin.transform.position;
        targetPosition = target.transform.position - originPosition;



        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(originPosition, targetPosition, out hit, Mathf.Infinity))
        {
            Debug.DrawRay(originPosition, targetPosition * hit.distance, Color.yellow);
            var targetRotation = Quaternion.LookRotation(targetPosition);
            Vector3 newDirection = Vector3.RotateTowards(origin.transform.forward, targetPosition, Mathf.Infinity, 0.0f);
            pointer.transform.rotation = Quaternion.LookRotation(newDirection);

            Debug.Log("Did Hit");
        }

        RaycastHit wallHit;
        if (Physics.Raycast(originPosition, targetPosition, out wallHit, Mathf.Infinity,23))
        {
            Debug.DrawRay(originPosition, targetPosition * hit.distance, Color.red);


            wallPosition = wallHit.point;
            wallPointer.transform.position = wallPosition;

            //LookDirection = Vector3.Reflect(originPosition , wallHit.normal);
            //Vector3 newDirection = Vector3.Reflect(wallHit.normal - originPosition, wallHit.normal.normalized);
     


            //newDirection

            //wallPointer.transform.RotateAround(newDirection, wallHit.normal, 180f);

            //wallPointer.transform.forward = newDirection.normalized;

            Ray reflectRay = new Ray(wallHit.point, Vector3.Reflect(originPosition - wallPosition.normalized, wallHit.normal));
            Debug.DrawRay(wallHit.point, Vector3.Reflect(wallPosition - originPosition.normalized, wallHit.normal) * hit.distance, Color.red);
            LookDirection = Vector3.Reflect(wallPosition - originPosition.normalized, wallHit.normal);
            wallPointer.transform.rotation = Quaternion.LookRotation(LookDirection);


            Debug.Log("Did Hit");
        }

        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
            Debug.Log("Did not Hit");
        }
    }
}
