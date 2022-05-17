using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipManager : MonoBehaviour
{

    public Transform TT_Start;
    public Transform TT_Mid;
    public Transform TT_End;
    public LineRenderer Linerenderer;

    [SerializeField] bool searchForTutorialObjects = false;
    [SerializeField] private GameObject canvasObject;

    private float sineSpeed = 1f;
    private float sineAmp = .1f;
    private float distance = 10;
    private bool hasChoppedIngredient = false;
    private bool hasCompletedTutorial = false;

    private GameObject closestObject = null;
    private GameObject highlightedObject = null;

    private Transform previousObject = null;
    private Transform objectToFollow = null;
    private Vector3 canvasHoverLocation = Vector3.zero;

    private List<TooltipObject> tooltipObjects = new List<TooltipObject>();

    private void Start()
    {
        tooltipObjects = TooltipObject.tooltipObjects;
        //ChoppingProcessable += OnFirstIngredientChopped;
    }

    void Update()
    {
        if (hasCompletedTutorial) return;

        if (hasChoppedIngredient) FinishTutorial();

        if (tooltipObjects != null && previousObject != objectToFollow)
        {
            previousObject = objectToFollow;
            canvasHoverLocation = objectToFollow.position;
        }

        objectToFollow = GetClosestGameObject().transform;

        HighlightObject();

        if (objectToFollow != previousObject)
        {
            canvasObject.transform.position = new Vector3(objectToFollow.position.x, objectToFollow.position.y + 1 + (Mathf.Sin(Time.time * sineSpeed) * sineAmp), objectToFollow.position.z);
        } else
        {
            canvasObject.transform.position = new Vector3(canvasHoverLocation.x, canvasHoverLocation.y + 1 + (Mathf.Sin(Time.time * sineSpeed) * sineAmp), canvasHoverLocation.z);

        }

        canvasObject.transform.LookAt(Camera.main.transform);
        TT_End.position = objectToFollow.transform.position;

    }

    private void OnFirstIngredientChopped()
    {
        hasChoppedIngredient = true;
        //ChoppingProcessable.OnFirstIngredientChopped -= OnFirstIngredientChopped;
    }

    private void FinishTutorial()
    {
        canvasObject.SetActive(false);
        HighlightObject();
        hasCompletedTutorial = true;
    }

    private GameObject GetClosestGameObject()
    {
        Vector3 playerPos = PhysicsCharacter.PhysicsPlayerBlackboard.Instance.headAnchor.position;

        if (closestObject != null)
        {
            distance = (playerPos - closestObject.transform.position).sqrMagnitude;
        }

        for (int i = 0; i < tooltipObjects.Count; i++)
        {
            float nextDistance = (playerPos - tooltipObjects[i].transform.position).sqrMagnitude;

            if (nextDistance < distance)
            {
                distance = nextDistance;
                closestObject = tooltipObjects[i].gameObject;
            }
        }
        return closestObject;
    }


    private void HighlightObject()
    {
        if (closestObject != highlightedObject || hasChoppedIngredient)
        {
            if (highlightedObject != null)
            {
                highlightedObject.gameObject.GetComponent<HighlightPlus.HighlightEffect>().highlighted = false;
            }
        }
        else
        {
            HighlightPlus.HighlightEffect highlightEffect = closestObject.gameObject.GetComponent<HighlightPlus.HighlightEffect>();

            highlightEffect.ProfileReload();
            highlightEffect.highlighted = true;
        }
        highlightedObject = closestObject;
    }
}
