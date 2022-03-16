using UnityEngine;
using UnityEngine.UI;

public class OrderDisplay : MonoBehaviour
{
    public OrderDisplayGrid grid;
    public Slider timeSlider;
    public Text number;

    public Order Order { get; private set; }

    public void DisplayOrder(Order order)
    {
        Order = order;
        grid.DisplayOrder(order);
        number.text = number.ToString();
    }

    public void Update()
    {
        timeSlider.value = Order.timer.TimeRemaining / Order.timer.Duration;
    }
}
