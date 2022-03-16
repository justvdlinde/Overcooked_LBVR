using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DeliveryPoint : MonoBehaviour
{
    public Action<Dish> PlateDeliveredEvent;

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Dish plate))
        {
            PlateDeliveredEvent?.Invoke(plate);
        }
    }
}
