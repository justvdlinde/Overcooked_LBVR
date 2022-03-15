using System;
/// <summary>
/// Interface for a networked client 
/// </summary>
public interface IClient
{
    public string UserId { get; }
    public string IpAddress { get; }
    public string DeviceID { get; }
    public bool IsLocal { get; }
}
