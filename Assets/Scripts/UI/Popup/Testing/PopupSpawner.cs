using UnityEngine;
using Utils.Core.Services;

public class PopupSpawner : MonoBehaviour
{
    [SerializeField] private KeyCode keyCode = KeyCode.Space;

    private PopupService popupService;

    private void Awake()
    {
        popupService = GlobalServiceLocator.Instance.Get<PopupService>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(keyCode))
        {
            InteractablePopup popup = popupService.SpawnPopup();
            InteractablePopup.ButtonData button1 = new InteractablePopup.ButtonData("Yes", () => {
                Debug.Log("Pressed Yes");
            });
            InteractablePopup.ButtonData button2 = new InteractablePopup.ButtonData("No", () => {
                Debug.Log("Pressed No");
            });

            popup.Setup("Error", "This is a test", button1, button2);
        }
    }
}
