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
    public override string Name => "Story Mode";
    public override GameModeEnum GameModeEnum => GameModeEnum.Story;
    public override OrdersController OrdersController => ordersController;
    public override IGameModeScoreboard Scoreboard => scoreboard;
    public TieredOrderGenerator OrderGenerator => orderGenerator;

    [SerializeField] private OrdersController ordersController = null;
    [SerializeField] private StoryModeScoreboard scoreboard = null;
    [SerializeField, InspectableSO] protected StorySettings settings;

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
        settings = Resources.Load<StorySettings>("StorySettings");
        settings = Instantiate(settings); // Make copy so the original asset stays the same

        if(PhotonNetwork.IsMasterClient)
            SetMatchDuration(settings.gameDuration);

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

    private IEnumerator StoryFlow()
    {
        yield return new WaitForSeconds(settings.initialStartDelay);

        while (GameTimer.IsRunning)
        {
            if (OrderDisplayManager.AllDisplaysAreFree())
            {
                OrderDisplay freeDisplay = OrderDisplayManager.GetFreeDisplay();
                CreateNewActiveOrder(freeDisplay.orderNumber);
                continue;
            }
            else
            { 
                float delay = Random.Range(settings.orderDelayMin, settings.orderDelayMax);
                yield return new WaitForSeconds(delay);
            }

            while (!OrderDisplayManager.HasFreeDisplay())
            {
                yield return null;
            }

            if (PhotonNetwork.IsMasterClient)
            {
                OrderDisplay freeDisplay = OrderDisplayManager.GetFreeDisplay();
                CreateNewActiveOrder(freeDisplay.orderNumber);
            }
        }
    }

    public void CreateNewActiveOrder(int displayNr)
    {
        // TODO: sync generator tier somehow
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
            Debug.Log("Delivered dish: " + dish.FoodStack + "\nclosest match: " + order);
        }
        else
        {
            throw new System.Exception("No closest order found!");
        }

        ScoreData score = scoreCalculator.CalculateScore(order, dish.FoodStack, DishResult.Delivered);
        Debug.Log("Score: " + score.Points + "/" + ScoreData.MaxPoints);
        OnDishDelivered(dish, order, score);
    }

    public void CheatDeliverDish(int displayNr)
    {
        Order order = ordersController.ActiveOrders.Where(o => o.orderNumber == displayNr).First();
        if (order != null)
        {
            ScoreData score = new ScoreData(ScoreData.MaxPoints, DishResult.Delivered);
            OnDishDelivered(null, order, score);
        }
    }

    private void OnDishDelivered(Plate dish, Order order, ScoreData score)
    {
        (Scoreboard as StoryModeScoreboard).AddScore(order, score);
        OrdersController.RemoveActiveOrder(order);
        order.TimerExceededEvent -= OnOrderTimerExceeded;
        orderGenerator.OnOrderCompleted(true);
        globalEventDispatcher.Invoke(new DishDeliveredEvent(dish, order, score));

        if (!GameTimer.IsRunning && ordersController.ActiveOrders.Count == 0)
            OnLastOrderServed();
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

        if (!GameTimer.IsRunning && ordersController.ActiveOrders.Count == 0)
            OnLastOrderServed();
    }

    private void OnLastOrderServed()
    {
        if (PhotonNetwork.IsMasterClient)
            EndGame();
    }
}
