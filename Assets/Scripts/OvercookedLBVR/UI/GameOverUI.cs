using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject root = null;
    [SerializeField] private Image[] stars = null;
    [SerializeField] private TextMeshProUGUI gamemodeTitle = null;
    [SerializeField] private TextMeshProUGUI orderCountLabel = null;
    [SerializeField] private TextMeshProUGUI ordersFailedLabel = null;
    [SerializeField] private TextMeshProUGUI perfectOrdersLabel = null;

    public void Show(IGameResult result)
    {
        GameMode gamemode = result.GameMode;
        IGameModeScoreboard score = result.Scores;

        root.SetActive(true);
        gamemodeTitle.text = gamemode.Name;
        orderCountLabel.text = "Orders: " + score.OrdersCount.ToString();
        ordersFailedLabel.text = "Failed: " + (score.OrdersCount - score.DeliveredOrdersCount).ToString();
        perfectOrdersLabel.text = "Perfect: " + score.PerfectOrdersCount.ToString();
        SetupStars(score.TotalPoints, score.MaxAchievablePoints);
    }

    public void Hide()
    {
        root.SetActive(false);
    }

    private void SetupStars(float points, float maxPoints)
    {
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].enabled = (points / maxPoints) >= (((float)i + 1) / stars.Length);
        }
    }
}
