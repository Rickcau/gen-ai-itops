using Azure.Core;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Net.Http.Json;
using Helper.AzureOpenAISearchConfiguration;

namespace Plugins
{
    public class WeatherResponse
    {
        public MainInfo? Weather { get; set; }
    }

    public class MainInfo
    {
        public double temp { get; set; }
        public double feels_like { get; set; }
    }

    public class WeatherPlugin
    {
        private readonly Configuration _configuration;
        private readonly TokenCredential _credential;
        private readonly ILogger<WeatherPlugin> _logger;
        string _weatherPluginEndpoint = string.Empty; 
        string _weatherAPIKey = string.Empty; 
        const string units = "imperial";

        private readonly HttpClient _client = new();


        public WeatherPlugin(Configuration configuration, ILogger<WeatherPlugin>? logger = null)
        {
            _configuration = configuration;
            _logger = logger ?? LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<WeatherPlugin>();
            _weatherPluginEndpoint = _configuration.WeatherEndpoint!;
            _weatherAPIKey = _configuration.WeatherApiKey!;
        }

        [KernelFunction]
        [Description("Retreives the current weather for any zip code in the US")]
        public async Task<string> GetWeatherAsync([Description("zip code for the weather")] string zipcode)
        {
            var openWeatherEndpoint = $@"{_weatherPluginEndpoint}{zipcode},us&appid={_weatherAPIKey}&units={units}";
            HttpRequestMessage request = new(HttpMethod.Get, openWeatherEndpoint);

            var response = await _client.SendAsync(request).ConfigureAwait(false);
            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return result;
        }
    }
}
