using CryptoXchange.Configuration;
using CryptoXchange.JsonRpc;

namespace CryptoXchange.DaemonInterface
{
    public class DaemonResponse<T>
    {
        public JsonRpcException Error { get; set; }
        public T Response { get; set; }
        public AuthenticatedNetworkEndpointConfig Instance { get; set; }
    }
}
