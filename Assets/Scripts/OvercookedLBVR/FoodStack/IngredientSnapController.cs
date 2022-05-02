using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

public class IngredientSnapController : MonoBehaviour
{
    private const float MIN_HEIGHT = 0.005f;
    private const float MIN_COLLIDER_DISTANCE_FOR_RESTACK = 0.3f;

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

    [SerializeField] private FoodStack lastStack = null;

    private void OnValidate()
    {
        if (ingredient == null)
            ingredient = GetComponentInParent<Ingredient>();

        if (rigidbodyView == null)
            rigidbodyView = GetComponentInParent<PhotonRigidbodyView>();
    }

    public void OnSnap(bool snap)
    {
        IsSnapped = snap;
        ToggleObjects(!snap);

        if (rigidbodyView != null)
            rigidbodyView.enabled = false;

        if (rigidbody != null)
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.isKinematic = snap;
            rigidbody.useGravity = !snap;
        }

        SnapEvent?.Invoke(IsSnapped);
    }

    public void SetLastStackCollider(FoodStack stack)
    {
        lastStack = stack;
    }

    private void Update()
    {
        if (lastStack != null)
        {
            if (Vector3.Distance(lastStack.transform.position, transform.position) > MIN_COLLIDER_DISTANCE_FOR_RESTACK)
                lastStack = null;
        }
    }

    public bool CanBeSnapped()
    {
        if (ingredient.State != IngredientStatus.Processed)
            return false;

        return lastStack == null;
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

#if UNITY_EDITOR
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

    private void OnDrawGizmos()
    {
        if (lastStack != null)
        {
            if (Vector3.Distance(lastStack.transform.position, transform.position) > MIN_COLLIDER_DISTANCE_FOR_RESTACK)
                Gizmos.color = Color.red;
            else
                Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, lastStack.transform.position);
        }
    }
#endif
}
