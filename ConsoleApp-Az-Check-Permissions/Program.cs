using Azure.Core;
using Azure.Identity;
using System.IdentityModel.Tokens.Jwt;
using AzureAutomationLibrary;
using System.Configuration;

// RDC: Work as of 12/20
// Purpose:  This demostrates our ability to collect the role/permissions for a user that was authenticated using
// Easy Auth.
// The important part about this, is the following:
// 1. Proper setup of the App Registration
// 2. Custom Roles need to Created
// 3. Assignment of Custom Roles to the users
// This is an important concept to grasp, because you need to leverage the App Registeration to get a bearer token for the user
// this credential (Token) will have the claims for the user and the permissions he or she can use.
//
// We will leverage these concepts for both the frontend and the backend API.
// Now that we are able to demostrate the concept of getting the claims for a user
// we can do two things:
// 1) The Client can choose to display features that the user has access to based on the roles assigned.
// 2) The Client will pass the token to the API and the API will forbid access to operations the user does not 
//    have access to.
// 

// Configuration

var ClientId = ConfigurationManager.AppSettings["GenAiAPIClientId"] 
    ?? throw new ConfigurationErrorsException("GenAi API ClientId not found in config");
var TenantId = ConfigurationManager.AppSettings["GenAiAPITenantId"]
    ?? throw new ConfigurationErrorsException("GenAi API TenantId not found in config");
var RedirectUri = ConfigurationManager.AppSettings["GenAiAPIRedirectUri"]
    ?? throw new ConfigurationErrorsException("GenAi APIRedirect Uri TenantId not found in config");



//const string ClientId = "e47b4848-430e-42c4-9bcc-e26a16c7faa0";
//const string TenantId = "0d8f4519-e72a-4019-8975-59754a33abb1";
//const string RedirectUri = "http://localhost";

var config = new RunbookConfig
{
    SubscriptionId = ConfigurationManager.AppSettings["AzureSubscriptionId"]
        ?? throw new ConfigurationErrorsException("AzureSubscriptionId not found in config"),
    ResourceGroupName = ConfigurationManager.AppSettings["AzureAutomationResourceGroupName"]
        ?? throw new ConfigurationErrorsException("AzureAutomationResourceGroupName not found in config"),
    AutomationAccountName = ConfigurationManager.AppSettings["AzureAutomationAccountName"]
        ?? throw new ConfigurationErrorsException("AzureAutomationAccountName not found in config"),
    RunbookName = ConfigurationManager.AppSettings["AzureRunbookName"]
        ?? throw new ConfigurationErrorsException("AzureRunbookName not found in config")
};

try
{

    //var credential = new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
    //{
    //    TokenCachePersistenceOptions = new TokenCachePersistenceOptions()
    //});

    var options = new InteractiveBrowserCredentialOptions
    {
        TenantId = TenantId,
        ClientId = ClientId,
        RedirectUri = new Uri(RedirectUri),
        TokenCachePersistenceOptions = new TokenCachePersistenceOptions()
    };

    var credential = new InteractiveBrowserCredential(options);

    Console.WriteLine("A browser window will open for authentication. Please sign in...");
    var token = await credential.GetTokenAsync(
        new TokenRequestContext(new[] { $"{ClientId}/.default" })
    );

    var handler = new JwtSecurityTokenHandler();
    var jwtToken = handler.ReadJwtToken(token.Token);

    var roles = jwtToken.Claims
        .Where(c => c.Type == "roles")
        .Select(c => c.Value)
        .ToList();

    Console.WriteLine("\nYour permissions from the token:");
    if (roles.Any())
    {
        foreach (var role in roles)
        {
            Console.WriteLine($"- {role}");
        }
    }
    else
    {
        Console.WriteLine("No roles found in the token.");
    }

    Console.WriteLine("\nToken details:");
    Console.WriteLine($"Name: {jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value}");
    Console.WriteLine($"Username: {jwtToken.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value}");
    Console.WriteLine($"Tenant: {jwtToken.Claims.FirstOrDefault(c => c.Type == "tid")?.Value}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    if (ex.InnerException != null)
    {
        Console.WriteLine($"Inner Error: {ex.InnerException.Message}");
    }
}