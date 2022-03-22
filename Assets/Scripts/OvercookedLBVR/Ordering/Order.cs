using System;

public class Order : IDisposable
{
    public IngredientType[] ingredients;
    public Timer timer;
    public int orderNumber;
    public Score score;

    public Order()
    {
        timer = new Timer();
        timer.onDone += OnTimerExceeded;
    }

    public void OnTimerExceeded()
    {
        OrderManager.Instance.OnOrderTimerExceeded(this);
    }

    public void Dispose()
    {
        timer.Stop();
        timer.onDone -= OnTimerExceeded;
    }
}
