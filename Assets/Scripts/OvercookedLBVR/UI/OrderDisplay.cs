using UnityEngine;
using UnityEngine.UI;

public class OrderDisplay : MonoBehaviour
{
    public OrderDisplayGrid grid;
    public Slider timeSlider;
    public int orderNumber;

    public Order Order { get; private set; }

    private void Awake()
    {
        timeSlider.value = 0;
    }

    public void Update()
    {
        if (Order != null)
            timeSlider.value = Order.timer.TimeRemaining / Order.timer.Duration;
    }

    public void Show(Order order)
    {
        // TODO: some kind of animation/flair
        Clear();
        order.orderNumber = orderNumber;
        Order = order;
        grid.DisplayOrder(order);
    }

    public void Clear()
    {
        // TODO: some kind of animation/flair
        grid.Clear();
        Order = null;
        timeSlider.value = 0;
    }

    public bool CanBeUsed()
    {
        return Order == null;
    }
}
