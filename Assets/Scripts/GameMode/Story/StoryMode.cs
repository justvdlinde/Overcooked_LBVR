using Photon.Pun;
using System.Linq;
using System.Collections;
using UnityEngine;
using Utils.Core.Attributes;
using Utils.Core.Services;
using Utils.Core;

public class StoryMode : GameMode
{
    public const string PrefabName = "StoryMode";
    public override string Name => "Story";
    public override GameModeEnum GameModeEnum => GameModeEnum.Story;
    public override OrdersController OrdersController => ordersController;
    public override IGameModeScoreboard Scoreboard => scoreboard;
    public TieredOrderGenerator OrderGenerator => orderGenerator;

    [SerializeField] private OrdersController ordersController = null;
    [SerializeField] private StoryModeScoreboard scoreboard = null;
    [SerializeField, InspectableSO] protected GameSettings settings;

    protected CoroutineService coroutineService;
    private TieredOrderGenerator orderGenerator;
    private ScoreCalculator scoreCalculator;
    private CoroutineTask storyFlow;

    public override void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        base.OnPhotonInstantiate(info);
        coroutineService = GlobalServiceLocator.Instance.Get<CoroutineService>();
    }

    private void OnEnable()
    {
        ordersController.ActiveOrderAdded += OnActiveOrderAdded;
    }

    private void OnDisable()
    {
        ordersController.ActiveOrderAdded -= OnActiveOrderAdded;
    }

    public override void Setup()
    {
        scoreCalculator = new ScoreCalculator();
        orderGenerator = new TieredOrderGenerator(true, 0);
        settings = Resources.Load<GameSettings>("GameSettings");

        // Make copy so we don't change the original asset's values:
        settings = Instantiate(settings);
        
        base.Setup();
    }

    public override void StartActiveGame()
    {
        base.StartActiveGame();
        storyFlow = coroutineService.StartCoroutine(StoryFlow());
    }

    public override void EndGame()
    {
        base.EndGame();
        if (storyFlow != null)
            storyFlow.Stop();

        if(PhotonNetwork.IsMasterClient)
            ordersController.RemoveAllActiveOrders();
    }

    // TODO: replace with some kind of state machine? Would be useful if we want narration in between orders
    // TODO: Get this to work on a set match duration instead of order amount
    private IEnumerator StoryFlow()
    {
        // TODO: sync up i when joining late
        for (int i = 0; i < settings.orderAmount; i++)
        {
            // TODO: get delay for next order instead of 1
            float delay = 1;
            yield return new WaitForSeconds(delay);

            while (!OrderDisplayManager.HasFreeDisplay())
            {
                yield return null;
            }

            yield return new WaitForSeconds(settings.nextOrderDelay);

            OrderDisplay freeDisplay = OrderDisplayManager.GetFreeDisplay();
            if (PhotonNetwork.IsMasterClient)
                CreateNewActiveOrder(freeDisplay.orderNumber);
        }
    }

    public void CreateNewActiveOrder(int displayNr)
    {
        Order order = orderGenerator.Generate();
        order.orderNumber = displayNr;
        OrdersController.AddActiveOrder(order);
    }

    public override IGameResult GetGameResult()
    {
        return new StoryGameResult(this, Scoreboard as StoryModeScoreboard);
    }

    public override void DeliverDish(Plate dish)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        Order order = OrdersController.GetClosestMatch(dish.FoodStack);

        if (order != null)
        {
            dish.OnDeliver();
            Debug.Log("Delivered dish: " + dish + "\nclosest match: " + order);
        }
        else
        {
            throw new System.Exception("No closest order found!");
        }

        ScoreData score = scoreCalculator.CalculateScore(order, dish.FoodStack, DishResult.Delivered);
        (Scoreboard as StoryModeScoreboard).AddScore(order, score);
        OrdersController.RemoveActiveOrder(order);
        order.TimerExceededEvent -= OnOrderTimerExceeded;
        orderGenerator.OnOrderCompleted(true);
        globalEventDispatcher.Invoke(new DishDeliveredEvent(dish, order, score));

        if (IsLastOrderDone())
            OnLastOrderServed();
    }

    public void CheatDeliverDish(int displayNr)
    {
        Order order = ordersController.ActiveOrders.Where(o => o.orderNumber == displayNr).First();
        if (order != null)
        {
            ScoreData score = new ScoreData(ScoreData.MaxPoints, DishResult.Delivered);
            (Scoreboard as StoryModeScoreboard).AddScore(order, score);
            OrdersController.RemoveActiveOrder(order);
            order.TimerExceededEvent -= OnOrderTimerExceeded;
            globalEventDispatcher.Invoke(new DishDeliveredEvent(null, order, score));

            if (IsLastOrderDone())
                OnLastOrderServed();
        }
    }

    private void OnActiveOrderAdded(Order order)
    {
        order.TimerExceededEvent += OnOrderTimerExceeded;
    }

    private void OnOrderTimerExceeded(Order order)
    {
        order.TimerExceededEvent -= OnOrderTimerExceeded;
        orderGenerator.OnOrderCompleted(false);

        if (PhotonNetwork.IsMasterClient)
        {
            OrdersController.RemoveActiveOrder(order);
            ScoreData score = scoreCalculator.CalculateScore(order, null, DishResult.TimerExceeded);
            (Scoreboard as StoryModeScoreboard).AddScore(order, score);
        }

        if (IsLastOrderDone())
            OnLastOrderServed();
    }

    private bool IsLastOrderDone()
    {
        return OrdersController.ActiveOrders.Count == 0 && OrdersController.CompletedOrders.Count >= settings.orderAmount;
    }

    private void OnLastOrderServed()
    {
        if (PhotonNetwork.IsMasterClient)
            EndGame();
    }
}
