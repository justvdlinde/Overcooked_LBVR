using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

public class IngredientSnapController : MonoBehaviour
{
    private const float MIN_HEIGHT = 0.01f;

    public Ingredient Ingredient => ingredient;
    public bool IsSnapped { get; private set; }

    public Action<bool> SnapEvent;

    [SerializeField] private Ingredient ingredient = null;
    [SerializeField] private new Rigidbody rigidbody = null;
    [Tooltip("Renderer used to calculate object height when stacking on dish, use processed version")]
    [SerializeField] private MeshFilter meshFilter = null;
    [SerializeField] private PhotonRigidbodyView rigidbodyView = null;
    [SerializeField] private List<GameObject> toggleObjects = new List<GameObject>();
    [SerializeField] private float heightMultiplier = 1;
    [SerializeField] private bool drawHeightGizmo = true;

    // TOOD: cleanup
    public bool canStack = true;
    public Transform recentDishCollider = null;

    private void OnValidate()
    {
        if (ingredient == null)
            ingredient = GetComponentInParent<Ingredient>();

        if (rigidbodyView == null)
            rigidbodyView = GetComponentInParent<PhotonRigidbodyView>();
    }

    // TOOD: cleanup
    private void Update()
    {
        if (!canStack && recentDishCollider != null)
        {
            if (Vector3.Distance(recentDishCollider.transform.position, transform.position) > 0.3f)
            {
                canStack = true;
                recentDishCollider = null;
            }
        }
    }

    public void OnSnap(bool snap)
    {
        IsSnapped = snap;
        SnapEvent?.Invoke(IsSnapped);
        ToggleObjects(!snap);

        if (rigidbodyView != null)
            rigidbodyView.enabled = false;

        if (rigidbody != null)
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.isKinematic = snap;
            rigidbody.useGravity = !snap;
        }
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
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            if (meshFilter == null)
            {
                Debug.LogWarning("meshFilter is null");
                return 0;
            }
            else if (meshFilter.sharedMesh == null)
            {
                meshFilter.sharedMesh = new Mesh();
            }
        }

        float objectHeight = meshFilter.sharedMesh.bounds.size.y * meshFilter.transform.localScale.y * heightMultiplier;

        if (objectHeight < MIN_HEIGHT)
            objectHeight = MIN_HEIGHT;

        return objectHeight;
    }

    [Utils.Core.Attributes.Button]
    private void PrintHeight()
    {
        Debug.Log(GetGraphicHeight());
    }

    private void OnDrawGizmosSelected()
    {
        if (drawHeightGizmo)
            Gizmos.DrawCube(transform.position, new Vector3(0.05f, GetGraphicHeight(), 0.05f));
    }
}
