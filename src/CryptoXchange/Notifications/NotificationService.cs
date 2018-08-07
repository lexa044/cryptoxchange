using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;

using System.Reactive.Concurrency;
using System.Reactive.Linq;

using Newtonsoft.Json;
using NLog;

using CryptoXchange.Infrastructure;
using CryptoXchange.Configuration;

namespace CryptoXchange.Notifications
{
    public class NotificationService
    {
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private readonly Regex regexStripHtml = new Regex(@"<[^>]*>", RegexOptions.Compiled);

        private readonly IContextHolder _contextHolder;
        private readonly BlockingCollection<QueuedNotification> _queue;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly HttpClient _httpClient;

        private IDisposable _queueSub;

        public NotificationService(IContextHolder contextHolder, JsonSerializerSettings jsonSerializerSettings)
        {
            _contextHolder = contextHolder;
            _jsonSerializerSettings = jsonSerializerSettings;
            _httpClient = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            });
            if (contextHolder.Config.Notifications?.Enabled == true)
            {
                _queue = new BlockingCollection<QueuedNotification>();

                _queueSub = _queue.GetConsumingEnumerable()
                    .ToObservable(TaskPoolScheduler.Default)
                    .Select(notification => Observable.FromAsync(() => SendNotificationAsync(notification)))
                    .Concat()
                    .Subscribe();
            }
        }

        public void NotifyPaymentSuccess(string coinId, decimal amount, int recpientsCount, string txInfo, decimal? txFee)
        {
            _queue?.Add(new QueuedNotification
            {
                Category = NotificationCategory.PaymentSuccess,
                PoolId = coinId,
                Subject = "Payout Success Notification",
                Msg = $"Paid {FormatAmount(amount, coinId)} {coinId} to {recpientsCount} recipients in Transaction(s) {txInfo}."
            });
        }

        public void NotifyPaymentFailure(string coinId, decimal amount, string message)
        {
            _queue?.Add(new QueuedNotification
            {
                Category = NotificationCategory.PaymentFailure,
                PoolId = coinId,
                Subject = "Payout Failure Notification",
                Msg = $"Failed to pay out {amount} {coinId}: {message}"
            });
        }

        public string FormatAmount(decimal amount, string coinId)
        {
            return $"{amount:0.#####}";
        }

        private async Task SendNotificationAsync(QueuedNotification notification)
        {
            _logger.Debug("SendNotificationAsync");

            try
            {
                if(notification.Category == NotificationCategory.PaymentFailure)
                {
                    if(_contextHolder.Config.Notifications?.DiscordNotifications?.Enabled == true)
                    {
                        DiscordNotifications discordNotifications = _contextHolder.Config.Notifications.DiscordNotifications;
                        await SendDiscordNotificationAsync(discordNotifications.WebHookUrl, notification.Msg, 
                            discordNotifications.Username,
                                discordNotifications.AvatarUrl);
                    }
                }
            }

            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        private async Task SendDiscordNotificationAsync(string webHookUrl, string msg, string username, string avatarUrl)
        {
            var postValues = JsonConvert.SerializeObject(new Dictionary<string, string>
                    {
                        {
                            "content", regexStripHtml.Replace(msg, string.Empty)
                        },
                        {"avatar_url", avatarUrl},
                        {"username", username}
                    });

            // build http request
            var request = new HttpRequestMessage(HttpMethod.Post, webHookUrl);
            request.Content = new StringContent(postValues, Encoding.UTF8, "application/json");

            // send request
            await _httpClient.SendAsync(request);
        }

        enum NotificationCategory
        {
            Admin,
            Block,
            PaymentSuccess,
            PaymentFailure,
        }

        struct QueuedNotification
        {
            public NotificationCategory Category;
            public string PoolId;
            public string Subject;
            public string Msg;
        }
    }
}
