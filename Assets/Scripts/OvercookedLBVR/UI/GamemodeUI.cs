using TMPro;
using UnityEngine;

public class GamemodeUI : MonoBehaviour
{
    [SerializeField] private GameObject root = null;
    [SerializeField] private TextMeshProUGUI gamemodeTitleLabel = null;

    public void Show(GameMode gamemode)
    {
        gamemodeTitleLabel.text = gamemode.Name;
        root.SetActive(true);
    }

    public void Hide()
    {
        root.SetActive(false);
    }
}
