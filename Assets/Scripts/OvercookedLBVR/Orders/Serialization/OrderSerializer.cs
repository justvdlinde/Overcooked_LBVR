using System;

public static class OrderSerializer
{
    /// <summary>
    /// Converts Order into serializable byte array
    /// </summary>
    /// <returns></returns>
    public static byte[] Serialize(Order order)
    {
        byte[] orderNr = BitConverter.GetBytes(order.orderNumber);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(orderNr);

        byte[] timerDuration = BitConverter.GetBytes(order.timer.Duration);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(timerDuration);

        byte[] timerStarted = BitConverter.GetBytes(order.timer.NetworkedTimeStarted);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(timerStarted);

        byte[] ingredients = ConvertIngredientsToByteArray(order.ingredients);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(ingredients);

        return ByteHelper.JoinBytes(orderNr, timerDuration, timerStarted, ingredients);
    }

    /// <summary>
    /// Deserializes byte array into a usable Order instance
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static Order Deserialize(byte[] data)
    {
        byte[] orderNrBytes = new byte[4];
        Array.Copy(data, 0, orderNrBytes, 0, orderNrBytes.Length);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(orderNrBytes);
        int orderNr = BitConverter.ToInt32(orderNrBytes, 0);
        int usedBytes = orderNrBytes.Length;

        byte[] timeDurationBytes = new byte[4];
        Array.Copy(data, timeDurationBytes.Length, timeDurationBytes, 0, timeDurationBytes.Length);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(timeDurationBytes);
        float timerDuration = BitConverter.ToSingle(timeDurationBytes, 0);
        usedBytes += timeDurationBytes.Length;

        byte[] timerStartedBytes = new byte[8];
        Array.Copy(data, timerStartedBytes.Length, timerStartedBytes, 0, timerStartedBytes.Length);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(timerStartedBytes);
        double timerStarted = BitConverter.ToDouble(timerStartedBytes, 0);
        usedBytes += timerStartedBytes.Length;

        byte[] ingredientBytes = new byte[data.Length - usedBytes];
        Array.Copy(data, usedBytes, ingredientBytes, 0, ingredientBytes.Length);
        IngredientType[] ingredients = ConvertByteArrayToIngredients(ingredientBytes);

        NetworkedTimer timer = new NetworkedTimer();
        timer.Set(timerStarted, timerDuration);
        Order order = new Order(orderNr, ingredients, timer);

        return order;
    }

    private static byte[] ConvertIngredientsToByteArray(IngredientType[] ingredients)
    {
        int[] ingredientInts = new int[ingredients.Length];
        for (int i = 0; i < ingredients.Length; i++)
        {
            ingredientInts[i] = (int)ingredients[i];
        }

        byte[] result = new byte[ingredientInts.Length * sizeof(int)];
        Buffer.BlockCopy(ingredientInts, 0, result, 0, result.Length);
        return result;
    }

    private static IngredientType[] ConvertByteArrayToIngredients(byte[] data)
    {
        if (BitConverter.IsLittleEndian)
            Array.Reverse(data);

        int[] ingredientInts = new int[data.Length / 4]; 
        Buffer.BlockCopy(data, 0, ingredientInts, 0, data.Length);

        IngredientType[] ingredients = new IngredientType[ingredientInts.Length];
        for (int i = 0; i < ingredients.Length; i++)
        {
            ingredients[i] = (IngredientType)ingredientInts[i];
        }
        return ingredients;
    }
}
