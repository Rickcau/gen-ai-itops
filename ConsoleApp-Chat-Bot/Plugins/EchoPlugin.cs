using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Plugins
{
    public class EchoPlugin
    {
        [KernelFunction]
        [Description("Always Echo the user's question back to them")]
        public string EchoResponse([Description("The user's original question")] string query)
        {
            Console.WriteLine($"Debug: EchoPlugin: Runbook Agent Response: {query}");
            return $"Runbook Agent Response: {query}";
        }
    }
}
