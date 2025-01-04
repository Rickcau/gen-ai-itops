using Microsoft.SemanticKernel;

namespace Helper.AgentSelection
{
    public static class AgentSelection
    {
        public static KernelFunction CreateSelectionFunction(string orchestratorAgentName, string runbookAgentName)
        {
            return KernelFunctionFactory.CreateFromPrompt(
                $$$"""
                Your job is to determine which participant takes the next turn in a conversation.
                Return only the name: "{{{orchestratorAgentName}}}" or "{{{runbookAgentName}}}".
                        
                Rules (in strict priority order):
                1. If this is a NEW user request (not a response to a status check), return "{{{orchestratorAgentName}}}"
                2. If the last message was a status check response from {{{runbookAgentName}}}, return "{{{orchestratorAgentName}}}" to ask if there's anything else
                3. If the user has responded "yes" to a status check and it hasn't been processed yet, return "{{{runbookAgentName}}}"
                4. After seeing "ROUTE_TO_SPECIALIST", return "{{{runbookAgentName}}}" until seeing "OPERATION_COMPLETE"
                5. After "OPERATION_COMPLETE", return "{{{orchestratorAgentName}}}" for wrap-up
                
                Check carefully:
                - Is this a status check response? ({{{runbookAgentName}}} just reported a status)
                - Is this a "yes" to check status? (User responding to "Would you like to check status?")
                - Has this request already been routed once?

                History:
                {{$chatHistory}}

                Return only the exact agent name, no explanation.
                """);
        }
    }
} 