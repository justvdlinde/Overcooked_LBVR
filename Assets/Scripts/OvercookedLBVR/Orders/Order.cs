using System;

public class Order : IDisposable
{
    public Action<Order> TimerExceededEvent;

    public readonly IngredientType[] ingredients;
    public readonly NetworkedTimer timer;
    public int orderNumber;

    public Order(IngredientType[] ingredients, NetworkedTimer timer) : this(-1, ingredients, timer) {  }

    public Order(int orderNumber, IngredientType[] ingredients, NetworkedTimer timer)
    {
        this.orderNumber = orderNumber;
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
        return base.ToString() + " Ingredients: " + string.Join("-", ingredients);
    }

    public void Dispose()
    {
        timer.Stop();
        timer.onDone -= OnTimerExceeded;
    }
}
