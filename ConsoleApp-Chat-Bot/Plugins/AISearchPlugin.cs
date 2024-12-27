using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Plugins
{
    public class AISearchPlugin
    {
        [KernelFunction, Description("Searches the Azure Search index for information about Runbooks that are available to perform operations with")]
        public async Task<string> SearchRunBookIndex([Description("The search query that we are searching the Azure Search Index for information matching the search query.")]
            string query)
        {
            Console.WriteLine($"Debug: AISearchPlugin: I can list VMs for you but I need to know the name of the resouce group... this is just for testing");
            await Task.Delay(1);
           return "AISearchPlugin: I can list VMs for you but I need to know the name of the resouce group... this is just for testing";
        }
    }
}
