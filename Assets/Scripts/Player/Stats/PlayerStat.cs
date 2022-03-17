using System;

/// <summary>
/// Abstract base class for a player statistic value
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class PlayerStat<T> : IDisposable where T : struct
{
    public delegate void ChangedEventHandler(T from, T to);
    public event ChangedEventHandler ChangedEvent;

    public T Value { get; protected set; }

    protected virtual void OnChanged(T from, T to)
    {
        ChangedEvent?.Invoke(from, to);
    }

    public abstract void Set(T value);

    public virtual void Dispose()
    {
        ChangedEvent = null;
    }

    public abstract void Reset();
}