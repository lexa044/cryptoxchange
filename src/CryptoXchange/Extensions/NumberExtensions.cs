using System;
using System.Net;

namespace CryptoXchange.Extensions
{
    public static class NumberExtensions
    {
        public static uint ToBigEndian(this uint value)
        {
            if (BitConverter.IsLittleEndian)
                return (uint)IPAddress.NetworkToHostOrder((int)value);

            return value;
        }

        public static uint ToLittleEndian(this uint value)
        {
            if (!BitConverter.IsLittleEndian)
                return (uint)IPAddress.HostToNetworkOrder((int)value);

            return value;
        }

        public static uint ReverseByteOrder(this uint value)
        {
            var bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);
            value = BitConverter.ToUInt32(bytes, 0);
            return value;
        }

        public static decimal TruncateDecimalPlaces(this decimal val, int places)
        {
            if (places <= 0)
                return Math.Truncate(val);

            return Math.Round(val - Convert.ToDecimal((0.5 / Math.Pow(10, places))), places);
        }
    }
}
