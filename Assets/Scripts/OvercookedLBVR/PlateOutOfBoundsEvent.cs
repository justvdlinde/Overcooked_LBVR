using System;
using Utils.Core.Events;

public class PlateOutOfBoundsEvent : IEvent
{
    public readonly Plate Plate;
    public readonly DateTime TimeStamp;

    public PlateOutOfBoundsEvent(Plate plate)
    {
        Plate = plate;
        TimeStamp = DateTime.Now;
    }
}
