using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using CryptoXchange.Configuration;
using CryptoXchange.JsonRpc;
using CryptoXchange.Util;

namespace CryptoXchange.DaemonInterface
{
    public class DaemonClient
    {
        public DaemonClient(JsonSerializerSettings serializerSettings, string walletPassword)
        {
            this.serializerSettings = serializerSettings;
            this.WalletPassword = walletPassword;
            serializer = new JsonSerializer
            {
                ContractResolver = serializerSettings.ContractResolver
            };
        }

        public string WalletPassword { get; set; }

        private readonly JsonSerializerSettings serializerSettings;
        private readonly JsonSerializer serializer;
        private DaemonEndpointConfig[] endPoints;
        private Dictionary<DaemonEndpointConfig, HttpClient> httpClients;

        #region API-Surface

        public void Configure(DaemonEndpointConfig[] endPoints, string digestAuthRealm = null)
        {
            this.endPoints = endPoints;

            // create one HttpClient instance per endpoint that carries the associated credentials
            httpClients = endPoints.ToDictionary(endpoint => endpoint, endpoint =>
            {
                var handler = new HttpClientHandler
                {
                    Credentials = new NetworkCredential(endpoint.User, endpoint.Password),
                    PreAuthenticate = true,
                    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
                };

                if (endpoint.Ssl && !endpoint.ValidateCert)
                {
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true;
                }

                return new HttpClient(handler);
            });
        }

        /// <summary>
        /// Executes the request against all configured demons and returns their responses as an array
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public Task<DaemonResponse<JToken>[]> ExecuteCmdAllAsync(string method)
        {
            return ExecuteCmdAllAsync<JToken>(method);
        }

        /// <summary>
        /// Executes the request against all configured demons and returns their responses as an array
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="method"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public async Task<DaemonResponse<TResponse>[]> ExecuteCmdAllAsync<TResponse>(string method,
            object payload = null, JsonSerializerSettings payloadJsonSerializerSettings = null)
            where TResponse : class
        {

            var tasks = endPoints.Select(endPoint => BuildRequestTask(endPoint, method, payload, payloadJsonSerializerSettings)).ToArray();

            try
            {
                await Task.WhenAll(tasks);
            }

            catch (Exception)
            {
                // ignored
            }

            var results = tasks.Select((x, i) => MapDaemonResponse<TResponse>(i, x))
                .ToArray();

            return results;
        }

        /// <summary>
        /// Executes the request against all configured demons and returns the first successful response
        /// </summary>
        /// <returns></returns>
        public Task<DaemonResponse<JToken>> ExecuteCmdAnyAsync(string method, bool throwOnError = false)
        {
            return ExecuteCmdAnyAsync<JToken>(method, null, null, throwOnError);
        }

        /// <summary>
        /// Executes the request against all configured demons and returns the first successful response
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <returns></returns>
        public async Task<DaemonResponse<TResponse>> ExecuteCmdAnyAsync<TResponse>(string method, object payload = null,
            JsonSerializerSettings payloadJsonSerializerSettings = null, bool throwOnError = false)
            where TResponse : class
        {

            var tasks = endPoints.Select(endPoint => BuildRequestTask(endPoint, method, payload, payloadJsonSerializerSettings)).ToArray();

            var taskFirstCompleted = await Task.WhenAny(tasks);
            var result = MapDaemonResponse<TResponse>(0, taskFirstCompleted, throwOnError);
            return result;
        }

        /// <summary>
        /// Executes the request against all configured demons and returns the first successful response
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public Task<DaemonResponse<JToken>> ExecuteCmdSingleAsync(string method)
        {
            return ExecuteCmdAnyAsync<JToken>(method);
        }

        /// <summary>
        /// Executes the request against all configured demons and returns the first successful response
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="method"></param>
        /// <param name="payload"></param>
        /// <returns></returns>
        public async Task<DaemonResponse<TResponse>> ExecuteCmdSingleAsync<TResponse>(string method, object payload = null,
            JsonSerializerSettings payloadJsonSerializerSettings = null)
            where TResponse : class
        {
            var task = BuildRequestTask(endPoints.First(), method, payload, payloadJsonSerializerSettings);
            await task;

            var result = MapDaemonResponse<TResponse>(0, task);
            return result;
        }

        /// <summary>
        /// Executes the requests against all configured demons and returns the first successful response array
        /// </summary>
        /// <returns></returns>
        public async Task<DaemonResponse<JToken>[]> ExecuteBatchAnyAsync(params DaemonCmd[] batch)
        {
            var tasks = endPoints.Select(endPoint => BuildBatchRequestTask(endPoint, batch)).ToArray();

            var taskFirstCompleted = await Task.WhenAny(tasks);
            var result = MapDaemonBatchResponse(0, taskFirstCompleted);
            return result;
        }
        #endregion // API-Surface


        private async Task<JsonRpcResponse> BuildRequestTask(DaemonEndpointConfig endPoint, string method, object payload,
            JsonSerializerSettings payloadJsonSerializerSettings = null)
        {
            var rpcRequestId = GetRequestId();

            // build rpc request
            var rpcRequest = new JsonRpcRequest<object>(method, payload, rpcRequestId);

            // build request url
            var protocol = endPoint.Ssl ? "https" : "http";
            var requestUrl = $"{protocol}://{endPoint.Host}:{endPoint.Port}";
            if (!string.IsNullOrEmpty(endPoint.HttpPath))
                requestUrl += $"{(endPoint.HttpPath.StartsWith("/") ? string.Empty : "/")}{endPoint.HttpPath}";

            // build http request
            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            var json = JsonConvert.SerializeObject(rpcRequest, payloadJsonSerializerSettings ?? serializerSettings);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            if (endPoint.Http2)
                request.Version = new Version(2, 0);

            // build auth header
            if (!string.IsNullOrEmpty(endPoint.User))
            {
                var auth = $"{endPoint.User}:{endPoint.Password}";
                var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(auth));
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64);
            }

            //Preventing deadlock: http://blog.stephencleary.com/2012/07/dont-block-on-async-code.html
            // send request
            using (var response = await httpClients[endPoint].SendAsync(request).ConfigureAwait(false))
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        using (var jreader = new JsonTextReader(reader))
                        {
                            var result = serializer.Deserialize<JsonRpcResponse>(jreader);
                            return result;
                        }
                    }
                }
            }
        }

        private async Task<JsonRpcResponse<JToken>[]> BuildBatchRequestTask(DaemonEndpointConfig endPoint, DaemonCmd[] batch)
        {
            // build rpc request
            var rpcRequests = batch.Select(x => new JsonRpcRequest<object>(x.Method, x.Payload, GetRequestId()));

            // build request url
            var protocol = endPoint.Ssl ? "https" : "http";
            var requestUrl = $"{protocol}://{endPoint.Host}:{endPoint.Port}";
            if (!string.IsNullOrEmpty(endPoint.HttpPath))
                requestUrl += $"{(endPoint.HttpPath.StartsWith("/") ? string.Empty : "/")}{endPoint.HttpPath}";

            // build http request
            using (var request = new HttpRequestMessage(HttpMethod.Post, requestUrl))
            {
                var json = JsonConvert.SerializeObject(rpcRequests, serializerSettings);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                if (endPoint.Http2)
                    request.Version = new Version(2, 0);

                // build auth header
                if (!string.IsNullOrEmpty(endPoint.User))
                {
                    var auth = $"{endPoint.User}:{endPoint.Password}";
                    var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(auth));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64);
                }

                //logger.Trace(() => $"Sending RPC request to {requestUrl}: {json}");

                // send request
                using (var response = await httpClients[endPoint].SendAsync(request))
                {
                    // check success
                    if (!response.IsSuccessStatusCode)
                        throw new DaemonClientException(response.StatusCode, response.ReasonPhrase);

                    // deserialize response
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        using (var reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            using (var jreader = new JsonTextReader(reader))
                            {
                                var result = serializer.Deserialize<JsonRpcResponse<JToken>[]>(jreader);
                                return result;
                            }
                        }
                    }
                }
            }
        }

        protected string GetRequestId()
        {
            var rpcRequestId = (DateTimeOffset.UtcNow.ToUnixTimeSeconds() + StaticRandom.Next(10)).ToString();
            return rpcRequestId;
        }

        private DaemonResponse<TResponse> MapDaemonResponse<TResponse>(int i, Task<JsonRpcResponse> x, bool throwOnError = false)
            where TResponse : class
        {
            var resp = new DaemonResponse<TResponse>
            {
                Instance = endPoints[i]
            };

            if (x.IsFaulted)
            {
                Exception inner;

                if (x.Exception.InnerExceptions.Count == 1)
                    inner = x.Exception.InnerException;
                else
                    inner = x.Exception;

                if (throwOnError)
                    throw inner;

                resp.Error = new JsonRpcException(-500, x.Exception.Message, null, inner);
            }

            else if (x.IsCanceled)
            {
                resp.Error = new JsonRpcException(-500, "Cancelled", null);
            }

            else
            {
                //Debug.Assert(x.IsCompletedSuccessfully);

                if (x.Result?.Result is JToken token)
                    resp.Response = token?.ToObject<TResponse>(serializer);
                else
                    resp.Response = (TResponse)x.Result?.Result;

                resp.Error = x.Result?.Error;
            }

            return resp;
        }

        private DaemonResponse<JToken>[] MapDaemonBatchResponse(int i, Task<JsonRpcResponse<JToken>[]> x)
        {
            if (x.IsFaulted)
                return x.Result?.Select(y => new DaemonResponse<JToken>
                {
                    Instance = endPoints[i],
                    Error = new JsonRpcException(-500, x.Exception.Message, null)
                }).ToArray();

            //Debug.Assert(x.IsCompletedSuccessfully);

            return x.Result?.Select(y => new DaemonResponse<JToken>
            {
                Instance = endPoints[i],
                Response = y.Result != null ? JToken.FromObject(y.Result) : null,
                Error = y.Error
            }).ToArray();
        }

    }
}
