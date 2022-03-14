/// <summary>
/// Class for an integer player statistic
/// </summary>
public class PlayerStatInt : PlayerStat<int>
{
    public PlayerStatInt(ChangedEventHandler changeCallback = null) : this(0, changeCallback) { }

    public PlayerStatInt(int amount, ChangedEventHandler changeCallback = null)
    {
        Value = amount;

        if (changeCallback != null)
            ChangedEvent += changeCallback;
    }

    public void Add(int addedAmount)
    {
        Value += addedAmount;
        OnChanged(Value - addedAmount, Value);
    }

    public override void Set(int value)
    {
        int prevAmount = Value;
        Value = value;
        OnChanged(prevAmount, Value);
    }

    public override void Reset()
    {
        Set(0);
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator int(PlayerStatInt stat) => stat.Value;
}
