using Photon.Pun;
using UnityEngine;
using Utils.Core.Events;
using Utils.Core.Services;

public class OrderResultManager : MonoBehaviourPun
{
    [SerializeField] private OrderResultUI resultPrefab = null;
    [SerializeField] private float yOffset = 0.4f;

    private GlobalEventDispatcher globalEventDispatcher;

    private void Awake()
    {
        globalEventDispatcher = GlobalServiceLocator.Instance.Get<GlobalEventDispatcher>();
    }

    private void OnEnable()
    {
        globalEventDispatcher.Subscribe<DishDeliveredEvent>(OnDishDeliveredEvent);
    }

    private void OnDisable()
    {
        globalEventDispatcher.Unsubscribe<DishDeliveredEvent>(OnDishDeliveredEvent);
    }

    private void OnDishDeliveredEvent(DishDeliveredEvent @event)
    {
        Vector3 spawnPos;
        if (@event.Dish == null)
        {
            spawnPos = transform.position;
        }
        else
        {
            spawnPos = @event.Dish.gameObject.transform.position;
            spawnPos.y += yOffset;
        }

        photonView.RPC(nameof(OnDeliveredUIRPC), RpcTarget.All, @event.Score.Points, @event.Score.Result, spawnPos, Quaternion.identity);
    }

    [PunRPC]
    public void OnDeliveredUIRPC(float points, DishResult result, Vector3 spawnPos, Quaternion rotation)
	{
        OrderResultUI instance = Instantiate(resultPrefab, spawnPos, Quaternion.identity);
        ScoreData scoreData = new ScoreData(points, result);
        instance.Setup(scoreData);
    }
}
