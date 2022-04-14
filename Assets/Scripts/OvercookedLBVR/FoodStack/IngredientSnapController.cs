using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

public class IngredientSnapController : MonoBehaviour
{
    private const float MIN_HEIGHT = 0.015f;

    public Ingredient Ingredient => ingredient;
    public bool IsSnapped { get; private set; }

    public Action<bool> SnapEvent;

    [SerializeField] private Ingredient ingredient = null;
    [Tooltip("Renderer used to calculate object height when stacking on dish, use processed version")]
    [SerializeField] private MeshFilter meshFilter = null;
    [SerializeField] private float heightMultiplier = 1;
    [SerializeField] private PhotonRigidbodyView rigidbodyView = null; 
    [SerializeField] private List<GameObject> toggleObjects = new List<GameObject>();

    //private bool canStack = true;
    //private Transform recentDishCollider = null;

    private void OnValidate()
    {
        if (ingredient == null)
            ingredient = GetComponentInParent<Ingredient>();

        if (rigidbodyView == null)
            rigidbodyView = GetComponentInParent<PhotonRigidbodyView>();
    }

    //private void Update()
    //{
    //    if (!canStack && recentDishCollider != null)
    //    {
    //        if (Vector3.Distance(recentDishCollider.transform.position, transform.position) > 0.3f)
    //        {
    //            canStack = true;
    //            recentDishCollider = null;
    //        }
    //    }
    //}

    public void OnSnap(bool snap)
    {
        IsSnapped = snap;
        SnapEvent?.Invoke(IsSnapped);
        rigidbodyView.enabled = false;
    }

    public bool CanBeSnapped()
    {
        return /*canStack && */ingredient.State == IngredientStatus.Processed;
    }

    private void ToggleObjects(bool active)
    {
        foreach (var item in toggleObjects)
        {
            item.SetActive(active);
        }
    }

    public float GetGraphicHeight()
    {
        float objectHeight = meshFilter.mesh.bounds.size.y * meshFilter.transform.localScale.y * heightMultiplier;

        if (objectHeight < MIN_HEIGHT)
            objectHeight = MIN_HEIGHT;

        return objectHeight;
    }
}
