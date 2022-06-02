using UnityEngine;
using UnityEngine.UI;

public class OrderDisplay : MonoBehaviour
{
    public OrderDisplayGrid grid;
    public Slider timeSlider;
    public int orderNumber;

    public Order Order => order;
    private Order order = null;

    private void Awake()
    {
        timeSlider.value = 0;
    }

    public void Update()
    {
        if (order != null)
            timeSlider.value = order.timer.TimeRemaining / order.timer.Duration;
    }

    public void Show(Order order)
    {
        // TODO: some kind of animation/flair
        Clear();
        this.order = order;
        this.order.orderNumber = orderNumber;
        grid.DisplayOrder(order);
    }

    public void Clear()
    {
        // TODO: some kind of animation/flair
        grid.Clear();
        order = null;
        timeSlider.value = 0;
    }

    public bool CanBeUsed()
    {
        return order == null;
    }
}
