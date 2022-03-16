using UnityEngine;
using UnityEngine.UI;

public class OrderDisplay : MonoBehaviour
{
    public OrderDisplayGrid grid;
    public Slider timeSlider;

    public Order Order { get; private set; } 

    public void DisplayOrder(Order order)
    {
        Order = order;
        grid.DisplayOrder(order);
    }

    public void Clear()
    {
        grid.Clear();
        Order = null;
    }

    public void Update()
    {
        if (Order != null)
            timeSlider.value = Order.timer.TimeRemaining / Order.timer.Duration;
    }
}
