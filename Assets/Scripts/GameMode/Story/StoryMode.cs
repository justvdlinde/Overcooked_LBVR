using Photon.Pun;
using System.Linq;
using System.Collections;
using UnityEngine;
using Utils.Core.Attributes;
using Utils.Core.Services;

public class StoryMode : GameMode
{
    public const string PrefabName = "StoryMode";
    public override string Name => "Story";
    public override GameModeEnum GameModeEnum => GameModeEnum.Story;
    public override OrdersController OrdersController => ordersController;
    public override IGameModeScoreboard Scoreboard => scoreboard;

    [SerializeField] private OrdersController ordersController = null;
    [SerializeField] private StoryModeScoreboard scoreboard = null;
    [SerializeField, InspectableSO] protected GameSettings settings;

    protected CoroutineService coroutineService;
    private TieredOrderGenerator orderGenerator;
    private ScoreCalculator scoreCalculator;

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
        orderGenerator = new TieredOrderGenerator();
        settings = Resources.Load<GameSettings>("GameSettings");

        // Make copy so we don't change the original asset's values:
        settings = Instantiate(settings);
        
        base.Setup();
    }

    public override void StartActiveGame()
    {
        base.StartActiveGame();
        globalEventDispatcher.Subscribe<DishDeliveredEvent>(OnDishDeliveredEvent);
        coroutineService.StartCoroutine(StoryFlow());
    }

    public override void EndGame()
    {
        base.EndGame();
        globalEventDispatcher.Unsubscribe<DishDeliveredEvent>(OnDishDeliveredEvent);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        globalEventDispatcher.Unsubscribe<DishDeliveredEvent>(OnDishDeliveredEvent);
    }

    // TODO: replace with some kind of state machine? Would be usefull if we want narration in between orders
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
            if(PhotonNetwork.IsMasterClient)
                CreateNewActiveOrder(freeDisplay.orderNumber);
        }
    }

    public void CreateNewActiveOrder(int displayNr)
    {
        Order order = orderGenerator.GenerateRandomOrder(3, 0, out int newTier, true);
        order.orderNumber = displayNr;
        order.timer.Set(10); // TODO: set timer
        OrdersController.AddActiveOrder(order);
    }

    public override IGameResult GetGameResult()
    {
        return new StoryGameResult(this, Scoreboard as StoryModeScoreboard);
    }

    private void OnDishDeliveredEvent(DishDeliveredEvent @event)
    {
        if (PhotonNetwork.IsMasterClient)
            DeliverDish(@event.Dish);
    }

    private void DeliverDish(FoodStack dish)
    {
        Order order = OrdersController.GetClosestMatch(dish);

        if (order != null)
        {
            dish.OnDeliver();
            Debug.Log("Delivered dish: " + dish + "\nclosest match: " + order);
        }
        else
        {
            throw new System.Exception("No closest order found!");
        }

        OrderScore score = scoreCalculator.CalculateScore(order, dish, DishResult.Delivered);
        (Scoreboard as StoryModeScoreboard).AddScore(order, score);
        OrdersController.RemoveActiveOrder(order);
        order.TimerExceededEvent -= OnOrderTimerExceeded;
    }

    public void CheatDeliverDish(int displayNr)
    {
        Order order = ordersController.ActiveOrders.Where(o => o.orderNumber == displayNr).First();
        if (order != null)
        {
            OrderScore score = new OrderScore(OrderScore.MaxPoints, DishResult.Delivered);
            (Scoreboard as StoryModeScoreboard).AddScore(order, score);
            OrdersController.RemoveActiveOrder(order);
            order.TimerExceededEvent -= OnOrderTimerExceeded;
        }
    }

    private void OnActiveOrderAdded(Order order)
    {
        order.TimerExceededEvent += OnOrderTimerExceeded;
    }

    private void OnOrderTimerExceeded(Order order)
    {
        order.TimerExceededEvent -= OnOrderTimerExceeded;

        if (PhotonNetwork.IsMasterClient)
        {
            OrdersController.RemoveActiveOrder(order);
            OrderScore score = scoreCalculator.CalculateScore(order, null, DishResult.TimerExceeded);
            (Scoreboard as StoryModeScoreboard).AddScore(order, score);
        }
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
