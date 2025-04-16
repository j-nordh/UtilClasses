using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using UtilClasses.Core.Extensions.Strings;

namespace UtilClasses.Core.Extensions.Net;

public static class NetExtensions
{
    public static IPAddress FirstV4Addr(this UnicastIPAddressInformationCollection col) => col.First(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).Address;
    public static UnicastIPAddressInformation FirstOrDefaultV4(this UnicastIPAddressInformationCollection col) => col.FirstOrDefault(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

    public static IPAddress WithLastByte(this IPAddress a, byte b) => a.Xor(0, 0, 0, b);
    public static IPAddress Xor(this IPAddress a, params byte[] bs)
    {
        var res = a.GetAddressBytes().Select((b, i) => (byte)(b | bs[i])).ToArray();
        return new IPAddress(res);
    }
    public static IPAddress GetBroadcastAddress(this IPAddress address, IPAddress subnetMask)
    {
        byte[] ipAdressBytes = address.GetAddressBytes();
        byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

        if (ipAdressBytes.Length != subnetMaskBytes.Length)
            throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

        byte[] broadcastAddress = new byte[ipAdressBytes.Length];
        for (int i = 0; i < broadcastAddress.Length; i++)
        {
            broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
        }
        return new IPAddress(broadcastAddress);
    }

    public static IPAddress GetNetworkAddress(this IPAddress address, IPAddress subnetMask)
    {
        byte[] ipAdressBytes = address.GetAddressBytes();
        byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

        if (ipAdressBytes.Length != subnetMaskBytes.Length)
            throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

        byte[] broadcastAddress = new byte[ipAdressBytes.Length];
        for (int i = 0; i < broadcastAddress.Length; i++)
        {
            broadcastAddress[i] = (byte)(ipAdressBytes[i] & (subnetMaskBytes[i]));
        }
        return new IPAddress(broadcastAddress);
    }

    public static bool IsInSameSubnet(this IPAddress address2, IPAddress address, IPAddress subnetMask)
    {
        IPAddress network1 = address.GetNetworkAddress(subnetMask);
        IPAddress network2 = address2.GetNetworkAddress(subnetMask);

        return network1.Equals(network2);
    }
}

public class NetUtil
{
    public static NetworkInterface GetNic(string address, out UnicastIPAddressInformation addr)
    {

        Func<UnicastIPAddressInformation, string, bool> cmp = address.EndsWith("0")
            ? (a, b) => a.Address.ToString().SubstringBeforeLast(".").EqualsOic(b.SubstringBeforeLast("."))
            : (a, b) => a.Address.ToString().EqualsOic(b);
        var nics = NetworkInterface.GetAllNetworkInterfaces()
            .Where(nic => nic.OperationalStatus == OperationalStatus.Up)
            .Where(nic => MatchNic(nic, address))
            .ToList();

        if (nics.Count != 1)
            throw nics.Count switch
            {
                0 => throw new ArgumentException("No such network interface found"),
                > 1 => throw new ArgumentException($"Too many ({nics.Count}) network interfaces found"),
                _ => throw new ArgumentOutOfRangeException("WTF?")
            };

        addr = GetIp(nics.First())!;
        return nics.First();
    }

    private static UnicastIPAddressInformation? GetIp(NetworkInterface nic) => nic.GetIPProperties()?.UnicastAddresses?.FirstOrDefaultV4();

    private static bool MatchNic(NetworkInterface nic, string matchAddress)
    {
        var nicIp = GetIp(nic);
        var nicAddress = nicIp?.Address?.ToString();
        if (nicAddress == null)
            return false;

        if (matchAddress.EndsWith(".0"))
        {
            nicAddress = nicAddress.SubstringBeforeLast(".");
            matchAddress = matchAddress.SubstringBeforeLast(".");
        }
        return nicAddress.EqualsOic(matchAddress);
    }
}