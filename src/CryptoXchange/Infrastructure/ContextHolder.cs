using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;

using CryptoXchange.Exchanges;
using CryptoXchange.Configuration;

namespace CryptoXchange.Infrastructure
{
    public class ContextHolder : IContextHolder
    {
        public ContextHolder(IConfiguration configuration, JsonSerializerSettings jsonSerializerSettings)
        {
            try
            {
                string file = configuration.GetConnectionString("filePath");
                Console.WriteLine($"Using configuration file {file}\n");

                var serializer = JsonSerializer.Create(jsonSerializerSettings);

                using (var reader = new StreamReader(file, Encoding.UTF8))
                {
                    using (var jsonReader = new JsonTextReader(reader))
                    {
                        Config = serializer.Deserialize<CXConfig>(jsonReader);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public CurrencyPair ExchangeRate { get; set; }
        public CXConfig Config { get; private set; }
    }
}
