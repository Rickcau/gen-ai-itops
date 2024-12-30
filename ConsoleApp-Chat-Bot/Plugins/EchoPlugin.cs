using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Plugins
{
    public class EchoPlugin
    {
        private readonly ILogger<EchoPlugin> _logger;

        public EchoPlugin(ILogger<EchoPlugin>? logger = null)
        {
            _logger = logger ?? LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<EchoPlugin>();
        }

        [KernelFunction]
        [Description("Echo the user's question back to them")]
        public string EchoResponse([Description("The user's original question")] string query)
        {
            _logger.LogDebug("EchoPlugin: Echoing response: {Query}", query);
            return $"Request details: \"{query}\"";
        }
    }
}
