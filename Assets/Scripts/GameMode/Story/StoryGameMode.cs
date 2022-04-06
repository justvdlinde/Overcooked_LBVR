using Photon.Pun;
using System.Collections;
using UnityEngine;
using Utils.Core;
using Utils.Core.Events;
using Utils.Core.Services;

public class StoryGameMode : GameMode
{
    public override string Name => "Story";
    public override GameModeEnum GameModeEnum => GameModeEnum.Story;

    protected CoroutineService coroutineService;
    protected GameSettings settings;

    private CoroutineTask orderDelay;

    public StoryGameMode(GlobalEventDispatcher globalEventDispatcher, INetworkService networkService, CoroutineService coroutineService) 
        : base(globalEventDispatcher, networkService) 
    {
        this.coroutineService = coroutineService;
    }

    public override void Setup()
    {
        Scoreboard = new StoryGameScores();
        settings = Resources.Load<GameSettings>("GameSettings");

        if (PhotonNetwork.IsMasterClient)
        {
            OrdersController = PhotonNetwork.InstantiateSceneObject("OrdersController", Vector3.zero, Quaternion.identity).GetComponent<OrdersController>();

            // make copy so we don't change the original asset's values
            settings = Object.Instantiate(settings);
            OrdersController.Initialize(settings);
        }
        else
        {
            OrdersController = OrdersController.Instance;
        }

        base.Setup();
    }

    public override void StartActiveGame()
    {
        base.StartActiveGame();
        Debug.Log("StartActiveGame");

        if (PhotonNetwork.IsMasterClient)
            coroutineService.StartCoroutine(StoryFlow());
    }

    public override void Shutdown()
    {
        base.Shutdown();

        if(PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(OrdersController.gameObject);
    }

    // TODO: all clients need to somehow execute this loop in case the masterclient disconnects
    // TODO: replace with some kind of state machine? Would be usefull if we want narration in between orders
    private IEnumerator StoryFlow()
    {
        for (int i = 0; i < settings.orderAmount; i++)
        {
            // TODO: get delay for next order instead of 1
            float delay = 1;
            yield return Wait(delay);

            while (!OrderDisplayManager.HasFreeDisplay())
            {
                yield return null;
            }

            yield return Wait(settings.nextOrderDelay);

            OrderDisplay freeDisplay = OrderDisplayManager.GetFreeDisplay();
            if(PhotonNetwork.IsMasterClient)
                OrdersController.CreateNewActiveOrder(freeDisplay.orderNumber);
        }
    }

    private IEnumerator Wait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

    public override IGameResult GetGameResult()
    {
        return new StoryGameResult(this, Scoreboard as StoryGameScores);
    }

    private bool IsLastOrderDone()
    {
        return OrdersController.ActiveOrders.Count == 0 && OrdersController.CompletedOrders.Count >= settings.orderAmount;
    }

    private void OnLastOrderServed()
    {
        Debug.LogFormat("Last dish served, Game Over");
        if (PhotonNetwork.IsMasterClient)
            EndGame();
    }
}
