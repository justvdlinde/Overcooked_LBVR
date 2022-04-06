using System;

public class Order : IDisposable
{
    public Action<Order> TimerExceededEvent;

    public readonly IngredientType[] ingredients;
    public readonly NetworkedTimer timer;
    public int orderNumber;

    public Order(IngredientType[] ingredients, NetworkedTimer timer)
    {
        //this.orderNumber = orderNumber;
        this.ingredients = ingredients;
        this.timer = timer;
        timer.onDone += OnTimerExceeded;
    }


    // TODO: remove
    //public Order(object[] data)
    //{
    //    try
    //    {
    //        orderNumber = (int)data[0];

    //        timer = new NetworkedTimer();
    //        timer.Set((float)data[1]);

    //        int[] ingredientInts = (int[])data[2];

    //        ingredients = new IngredientType[ingredientInts.Length];
    //        for (int i = 0; i < ingredients.Length; i++)
    //        {
    //            ingredients[i] = (IngredientType)ingredientInts[i];
    //        }
    //    }
    //    catch(Exception e)
    //    {
    //        throw new Exception("Something went wrong trying to convert object[] to an instance of type Order: " + e);
    //    }
    //}

    //public object[] GetPhotonSerializableData()
    //{
    //    int[] ingredientInts = new int[ingredients.Length];
    //    for (int i = 0; i < ingredients.Length; i++)
    //    {
    //        ingredientInts[i] = (int)ingredients[i];
    //    }

    //    object[] data = new object[]
    //    {
    //        orderNumber,
    //        timer.Duration,
    //        ingredientInts
    //    };

    //    return data;
    //}

    private void OnTimerExceeded()
    {
        TimerExceededEvent?.Invoke(this);
    }

    public void Dispose()
    {
        timer.Stop();
        timer.onDone -= OnTimerExceeded;
    }
}
