using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils.Core.Attributes;
using Utils.Core.Events;
using Utils.Core.Injection;
using Utils.Core.SceneManagement;
using HashTable = ExitGames.Client.Photon.Hashtable;

public abstract class GameFlow : MonoBehaviour
{
    [Tooltip("Short delay before scene starts to actually load, this allows for a screen fade for example")]
    [SerializeField] protected float sceneLoadDelay = 0.1f;

    [Tooltip("The initial level that will be loaded")]
    [SerializeField] protected GameLevel initialLevel = null;
    [SerializeField] protected GameModeEnum gameMode = GameModeEnum.Story;

    [SerializeField] protected bool showSplashScreen = true;
    [SerializeField, InspectableSO] private NetworkConfig networkConfig = null;
    [SerializeField] protected LoadingScreen loadingScreenPrefab = null;
    [SerializeField] protected Popup connectingScreenPrefab = null;

    protected DependencyInjector injector;
    protected INetworkService networkService;
    protected GlobalEventDispatcher globalEventDispatcher;
    protected GameModeService gameModeService;
    protected SceneService sceneService;
    protected PopupService popupService;

    protected bool wasConnected;
    protected Popup connectingScreenInstance;

    protected void InjectDependencies(DependencyInjector injector, INetworkService networkService, GlobalEventDispatcher globalEventDispatcher, 
        GameModeService gameModeService, SceneService sceneService, PopupService popupService)
    {
        this.injector = injector;
        this.networkService = networkService;
        this.globalEventDispatcher = globalEventDispatcher;
        this.gameModeService = gameModeService;
        this.sceneService = sceneService;
        this.popupService = popupService;
    }

    protected virtual void Awake()
    {
        Application.runInBackground = true;
        DontDestroyOnLoad(gameObject);

        // TODO: spawn splashscreen
        InstantiatePrefab(loadingScreenPrefab.gameObject);
        connectingScreenInstance = InstantiatePrefab(connectingScreenPrefab.gameObject).GetComponent<Popup>();
    }

    protected virtual void Start()
    {
        if (showSplashScreen)
            StartCoroutine(ShowSplashScreen(() => StartCoroutine(ConnectToNetwork())));
        else
            StartCoroutine(ConnectToNetwork());
    }

    public virtual void OnEnable()
    {
        globalEventDispatcher.Subscribe<ConnectionSuccessEvent>(OnConnectionSuccessEvent);
        globalEventDispatcher.Subscribe<ConnectionFailedEvent>(OnConnectionFailedEvent);
        PhotonNetworkService.RoomPropertiesChangedEvent += OnRoomPropertiesChangedEvent;
    }

    public virtual void OnDisable()
    {
        globalEventDispatcher.Unsubscribe<ConnectionFailedEvent>(OnConnectionFailedEvent);
        globalEventDispatcher.Unsubscribe<ConnectionSuccessEvent>(OnConnectionSuccessEvent);
        PhotonNetworkService.RoomPropertiesChangedEvent -= OnRoomPropertiesChangedEvent;
    }

    protected virtual GameObject InstantiatePrefab(GameObject prefab, bool dontDestroyOnLoad = true)
    {
        GameObject instance = injector.InstantiateGameObject(prefab);
        if(dontDestroyOnLoad)
            DontDestroyOnLoad(instance.gameObject);
        return instance;
    }

    protected virtual IEnumerator ShowSplashScreen(Action onDone)
    {
        yield return null;
        onDone?.Invoke();
    }

    // Needs to be IEnumerator, as connecting through Photon will otherwise not work when called from Awake or Start
    protected virtual IEnumerator ConnectToNetwork()
    {
        Debug.Log("ConnectToNetwork");
        yield return null;
        networkService.Connect(networkConfig);
        connectingScreenInstance.Open();
    }

    protected virtual void OnConnectionSuccessEvent(ConnectionSuccessEvent @event)
    {
        Debug.Log("OnConnectionSuccessEvent");
        wasConnected = true;
        connectingScreenInstance.Close();

        if (PhotonGameModeHelper.ServerHasGameMode(out GameModeEnum serverGameMode))
        {
            if (PhotonGameModeHelper.ServerGamemodeEqualsCurrentGamemode(gameModeService.CurrentGameMode))
                gameModeService.CurrentGameMode.OnReconnect();
            else
                gameModeService.StartNewGame(serverGameMode);
        }
        else
        {
            gameModeService.StartNewGame(gameMode);
        }

        if (@event.Room.CustomProperties.TryGetValue(RoomPropertiesPhoton.SCENE, out object sceneValue))
            OnScenePropertyChanged(sceneValue);
        else
            LoadScene(initialLevel.Scene.SceneName);
    }

    protected virtual void OnConnectionFailedEvent(ConnectionFailedEvent @event)
    {
        connectingScreenInstance.Close();
        if (Application.isPlaying)
        {
            string title = wasConnected ? "Lost connection" : "Failed to connect. ";
            popupService.SpawnPopup().Setup(title, @event.Info.ErrorDescription, new InteractablePopup.ButtonData("Retry", () => StartCoroutine(ConnectToNetwork())));
        }
        wasConnected = false;
    }

    protected virtual void OnRoomPropertiesChangedEvent(HashTable properties)  
    {
        if (properties.TryGetValue(RoomPropertiesPhoton.SCENE, out object sceneValue))
            OnScenePropertyChanged(sceneValue);
    }

    protected virtual void OnScenePropertyChanged(object sceneValue)
    {
        string sceneName = "";

        if (sceneValue is int)
            sceneName = SceneManager.GetSceneAt((int)sceneValue).name;
        else if (sceneValue is string)
            sceneName = (string)sceneValue;

        if (SceneManager.GetActiveScene().name != sceneName)
            LoadScene(sceneName);
    }

    protected virtual void LoadScene(string sceneName)
    {
        Debug.Log("LoadScene " + sceneName);
        if (sceneLoadDelay > 0)
            StartCoroutine(LoadSceneDelayed(sceneName, sceneLoadDelay));
        else
            sceneService.LoadSceneAsync(sceneName);
    }

    protected virtual IEnumerator LoadSceneDelayed(string sceneName, float seconds, Action onDone = null)
    {
        globalEventDispatcher.Invoke(new StartDelayedSceneLoadEvent(sceneName, seconds));
        yield return new WaitForSeconds(seconds);
        onDone?.Invoke();
        sceneService.LoadSceneAsync(sceneName);
    }
}