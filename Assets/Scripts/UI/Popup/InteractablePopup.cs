using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractablePopup : Popup
{
    public class ButtonData
    {
        public readonly string text;
        public readonly Action onClick;

        public ButtonData(string text, Action onClick)
        {
            this.text = text;
            this.onClick = onClick;
        }
    }

    [SerializeField] private PopupButton buttonPrefab = null;
    [SerializeField] private LayoutGroup buttonGroup = null;
    [SerializeField] private TextMeshProUGUI title = null;
    [SerializeField] private TextMeshProUGUI content = null;

    public void Setup(string title, string content, params ButtonData[] buttons)
    {
        this.title.text = title;
        this.content.text = content;

        foreach(Transform t in buttonGroup.transform)
        {
            Destroy(t.gameObject);
        }

        foreach(ButtonData data in buttons)
        {
            SpawnButton(data);
        }
    }

    private void SpawnButton(ButtonData data)
    {
        PopupButton button = Instantiate(buttonPrefab, buttonGroup.transform);
        button.Text.text = data.text;
        button.Button.onClick.AddListener(() => Close());
        button.Button.onClick.AddListener(() => data.onClick?.Invoke());
    }
}
