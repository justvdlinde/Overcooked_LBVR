using System;
using Utils.Core.Attributes;

public class Order : IDisposable
{
    public Action<Order> TimerExceededEvent;

    public IngredientType[] ingredients;
    public readonly NetworkedTimer timer;
    [ReadOnly] public int orderIndex;

    public Order(IngredientType[] ingredients, NetworkedTimer timer) : this(-1, ingredients, timer) {  }

    public Order(int orderNumber, IngredientType[] ingredients, NetworkedTimer timer)
    {
        this.orderIndex = orderNumber;
        this.ingredients = ingredients;
        this.timer = timer;
        timer.onDone += OnTimerExceeded;
    }

    private void OnTimerExceeded()
    {
        TimerExceededEvent?.Invoke(this);
    }

    public override string ToString()
    {
        return base.ToString() + " #" + orderIndex + " Ingredients: " + string.Join("-", ingredients);
    }

    public void Dispose()
    {
        timer.Stop();
        timer.onDone -= OnTimerExceeded;
    }
}
