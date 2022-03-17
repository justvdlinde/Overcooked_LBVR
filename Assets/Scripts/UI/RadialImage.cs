using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialImage : MonoBehaviour
{
    [SerializeField] private Image image = null;
    [SerializeField] private float fillSpeed = 1;
    [SerializeField] private float rotateSpeed = 10;

    private void Update()
    {
        image.fillAmount = (image.fillAmount + (fillSpeed * Time.deltaTime)) % 1;
        image.gameObject.transform.Rotate(-image.transform.forward, rotateSpeed * Time.deltaTime);
    }
}
