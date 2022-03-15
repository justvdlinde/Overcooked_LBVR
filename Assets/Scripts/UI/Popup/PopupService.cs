using UnityEngine;
using Utils.Core.Injection;
using Utils.Core.Services;

/// <summary>
/// Service for creating generic popups
/// </summary>
public class PopupService : IService
{
    private const string worldSpacePopupPrefab = "Popup - WorldSpace";
    private const string screenSpacePopupPrefab = "Popup - ScreenSpace";

    private DependencyInjector injector;

    public PopupService(DependencyInjector injector)
    {
        this.injector = injector;
    }

    public InteractablePopup SpawnPopup()
    {
        string prefabPath;
        if(GamePlatform.GameType == ClientType.Player)
            prefabPath = worldSpacePopupPrefab;
        else
            prefabPath = screenSpacePopupPrefab;

        InteractablePopup instance = Object.Instantiate(Resources.Load<InteractablePopup>(prefabPath));
        injector.InjectGameObject(instance.gameObject);
        return instance;
    }
}
