using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;
using Utils.Core.Services;

public class OrderBell : MonoBehaviour
{
    public Action PressEvent;

    [SerializeField] private PhysicsButton button = null;

    private void OnEnable()
    {
        button.PressEvent += OnPressEvent;
    }

    private void OnDisable()
    {
        button.PressEvent -= OnPressEvent;
    }

    private void OnPressEvent()
    {
        PressEvent?.Invoke();
    }
}
