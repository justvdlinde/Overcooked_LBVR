using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine;

public static class MacAddressHelper
{
    public static string GetMacAddress()
    {
#if OPERATOR_MOBILE
    return SystemInfo.deviceUniqueIdentifier;
#endif

#if OPERATOR_PC
        string macAddr = (
            from nic in NetworkInterface.GetAllNetworkInterfaces()
            where nic.OperationalStatus == OperationalStatus.Up
            select nic.GetPhysicalAddress().ToString()
        ).FirstOrDefault();

        return macAddr;
#endif

#pragma warning disable CS0162
        return string.Empty;
#pragma warning restore CS0162
    }
}
