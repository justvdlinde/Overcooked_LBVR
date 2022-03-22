using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderSummary : MonoBehaviour
{

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI orderName;
    [SerializeField] private TextMeshProUGUI summaryText;
    [SerializeField] private GameObject scorePanel;

    [Header("Star Scores")]
    [SerializeField] private float oneStarScore;
    [SerializeField] private float twoStarScore;
    [SerializeField] private float threeStarScore;

    [SerializeField] private Color starColorYellow;
    [SerializeField] private Color starColorBlack;
    [SerializeField] private List<Image> stars = new List<Image>();

    // Start is called before the first frame update
    void Start()
    {
        OrderManager.Instance.OrderDelivered += OnOrderDelivered;
    }

    private void OnOrderDelivered(Order order, Dish dish)
    {
        StartCoroutine((DelayedScoreText(() =>
        {
            scorePanel.SetActive(true);
            CalculateDeliveryMessage(order);
            CalculateStarScore(order);
            StartCoroutine((CloseScorePopup(() => { scorePanel.SetActive(false); })));
        })));
    }

    private IEnumerator DelayedScoreText(Action callback)
    {
        yield return new WaitForSeconds(.5f);
        callback();
    }

    private IEnumerator CloseScorePopup(Action callback)
    {
        yield return new WaitForSeconds(3);
        callback();
    }

    private void CalculateDeliveryMessage(Order order)
    {
        orderName.text = ("Order " + order.orderNumber.ToString());

        if (order.score.IsPerfectScore == true)
        {
            summaryText.text = "Perfect Meal!";
        } else if (order.score.ProperlyCookedIngredientsPercentage == 1 && order.score.IngredientsAreInCorrectOrder == false){
            summaryText.text = "Ingredients are in the wrong order!";
        } else
        {
            summaryText.text = "Missing Ingredients!";
        }
    }

    private void CalculateStarScore (Order order)
    {
        stars[0].color = order.score.Points >= oneStarScore ? starColorYellow : starColorBlack;
        stars[1].color = order.score.Points >= twoStarScore ? starColorYellow : starColorBlack;
        stars[2].color = order.score.Points >= threeStarScore ? starColorYellow : starColorBlack;
    }


}
