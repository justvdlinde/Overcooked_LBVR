using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tooltip : MonoBehaviour
{
    public Transform TT_Start;
    public Transform TT_Mid;
    public Transform TT_End;
    public LineRenderer Linerenderer;

    public Transform objectToFollow;

    [SerializeField] private GameObject canvasObject;
    private float sineSpeed = 1f;
    private float sineAmp = .1f;


    void Start()
    {
        
    }

    void Update()
    {
        canvasObject.transform.position = new Vector3(transform.position.x, transform.position.y + (Mathf.Sin(Time.time * sineSpeed) * sineAmp), transform.position.z);

        TT_Start.position = canvasObject.transform.position;
        TT_End.position = objectToFollow.transform.position;  

        //canvasObject.transform.LookAt(Camera.main.transform.position);
    }
}
