using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils.Core.Attributes;

public class PopupButton : MonoBehaviour
{
    public TextMeshProUGUI Text => text;
    [SerializeField] private TextMeshProUGUI text = null;

    public Button Button => button;
    [SerializeField] private Button button = null;

    private void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }

#if UNITY_EDITOR
    [Button]
    private void Click()
    {
        button.onClick.Invoke();
    }
#endif
}
