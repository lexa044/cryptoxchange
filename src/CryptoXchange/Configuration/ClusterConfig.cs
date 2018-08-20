using System.Collections.Generic;
using Newtonsoft.Json;

namespace CryptoXchange.Configuration
{
    public enum CoinType
    {
        BTC = 1, // Bitcoin
        BCH, // Bitcoin Cash
        LTC, // Litecoin
        DOGE, // Dogecoin,
        XMR, // Monero
        GRS, // GroestlCoin
        DGB, // Digibyte
        NMC, // Namecoin
        VIA, // Viacoin
        PPC, // Peercoin
        ZEC, // ZCash
        ZCL, // ZClassic
        ZEN, // Zencash
        ETH, // Ethereum
        ETC, // Ethereum Classic
        EXP, // Expanse
        DASH, // Dash
        MONA, // Monacoin
        VTC, // Vertcoin
        BTG, // Bitcoin Gold
        GLT, // Globaltoken
        ELLA, // Ellaism
        AEON, // AEON
        STAK, // Straks
        ETN, // Electroneum
        MOON, // MoonCoin
        XVG,  // Verge
        GBX,  // GoByte
        CRC,  // CrowdCoin
        BTCP, // Bitcoin Private
        CLO,  // Callisto
        FLO, // Flo
        PAK, // PAKcoin
        CANN, // CannabisCoin
        RVN,  // Ravencoin
        PGN,  // Pigeoncoin 
        BTP,  // Betchip
    }

    public class CoinConfig
    {
        public CoinType Type { get; set; }
        public string Algorithm { get; set; }
        public string WalletPassword { get; set; }

        public DaemonEndpointConfig[] Daemons { get; set; }
        public ZmqPubSubEndpointConfig[] ZmqTopics { get; set; }
        public int TransferRefreshInterval { get; set; }
    }

    public class ClusterLoggingConfig
    {
        public string Level { get; set; }
        public bool EnableConsoleLog { get; set; }
        public bool EnableConsoleColors { get; set; }
        public string LogFile { get; set; }
        public bool PerPoolLogFile { get; set; }
        public string LogBaseDirectory { get; set; }
    }

    public class NetworkEndpointConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }
    }

    public class AuthenticatedNetworkEndpointConfig : NetworkEndpointConfig
    {
        public string User { get; set; }
        public string Password { get; set; }
    }

    public class ZmqPubSubEndpointConfig
    {
        public string Url { get; set; }
        public string Topic { get; set; }
    }

    public class DaemonEndpointConfig : AuthenticatedNetworkEndpointConfig
    {
        /// <summary>
        /// Use SSL to for RPC requests
        /// </summary>
        public bool Ssl { get; set; }

        /// <summary>
        /// Use HTTP2 protocol for RPC requests (don't use this unless your daemon(s) live behind a HTTP reverse proxy)
        /// </summary>
        public bool Http2 { get; set; }

        /// <summary>
        /// Validate SSL certificate (if SSL option is set to true)
        /// </summary>
        public bool ValidateCert { get; set; }

        /// <summary>
        /// Optional endpoint category
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Optional request path for RPC requests
        /// </summary>
        public string HttpPath { get; set; }

        [JsonExtensionData]
        public IDictionary<string, object> Extra { get; set; }
    }

    public class DatabaseConfig : AuthenticatedNetworkEndpointConfig
    {
        public string Database { get; set; }
    }

    public class PersistenceConfig
    {
        public DatabaseConfig Postgres { get; set; }
    }

    public class EmailSenderConfig : AuthenticatedNetworkEndpointConfig
    {
        public string FromAddress { get; set; }
        public string FromName { get; set; }
    }

    public class AdminNotifications
    {
        public bool Enabled { get; set; }
        public string EmailAddress { get; set; }
        public bool NotifyBlockFound { get; set; }
        public bool NotifyPaymentSuccess { get; set; }
    }

    public class DiscordNotifications
    {
        public bool Enabled { get; set; }
        public string WebHookUrl { get; set; }
        public string Username { get; set; }
        public string AvatarUrl { get; set; }
    }

    public class NotificationsConfig
    {
        public bool Enabled { get; set; }

        public EmailSenderConfig Email { get; set; }
        public AdminNotifications Admin { get; set; }
        public DiscordNotifications DiscordNotifications { get; set; }
    }

    public class CXConfig
    {
        public string ExchangeName { get; set; }
        public decimal ExchangeValue { get; set; }
        public ClusterLoggingConfig Logging { get; set; }
        public NotificationsConfig Notifications { get; set; }
        public PersistenceConfig Persistence { get; set; }
        public CoinConfig[] Coins { get; set; }
    }
}
