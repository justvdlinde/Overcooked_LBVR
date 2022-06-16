using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Attributes;
using Utils.Core.Services;

[RequireComponent(typeof(Collider))]
public class DeliveryPoint : MonoBehaviour
{
    public int OrderNr { get; private set; }
    public OrderDisplay Display => display;

    [SerializeField] private OrderBell bell;
    [SerializeField] private TMPro.TextMeshProUGUI orderNrLabel;
    [SerializeField] private OrderDisplay display;

    private List<Plate> platesInTrigger = new List<Plate>();
    private GameModeService gamemodeService;

    private void Awake()
    {
        gamemodeService = GlobalServiceLocator.Instance.Get<GameModeService>();
    }

    public void SetOrderNr(int nr)
    {
        OrderNr = nr;
        orderNrLabel.text = (OrderNr + 1).ToString();
    }

    private void OnEnable()
    {
        bell.PressEvent += DeliverDishInTrigger;
    }

    private void OnDisable()
    {
        bell.PressEvent -= DeliverDishInTrigger;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Plate plate))
        {
            if (!platesInTrigger.Contains(plate))
            {
                platesInTrigger.Add(plate);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Plate plate))
        {
            if (platesInTrigger.Contains(plate))
            {
                platesInTrigger.Remove(plate);
            }
        }
    }

    [Button]
    public void DeliverDishInTrigger()
    {
        for (int i = 0; i < platesInTrigger.Count; i++)
        {
            Plate plate = platesInTrigger[i];
            if (plate == null)
            {
                platesInTrigger.Remove(plate);
                continue;
            }

            Debug.Log("plate.CanBeDelivered " + plate.CanBeDelivered());
            if (plate.CanBeDelivered())
            {
                DeliverDish(plate, OrderNr);
                break;
            }
        }
    }

    public void DeliverDish(Plate dish, int orderNr)
    {
        if(gamemodeService.CurrentGameMode != null)
            gamemodeService.CurrentGameMode.DeliverDish(dish, orderNr);

        platesInTrigger.Remove(dish);
        dish.OnDeliver();
    }
}
