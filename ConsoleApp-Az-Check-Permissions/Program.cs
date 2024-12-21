using Azure.Core;
using Azure.Identity;
using System.IdentityModel.Tokens.Jwt;

// RDC: Work as of 12/20
// Purpose:  This demostrates our ability to collect the role/permissions for a user that was authenticated using
// Easy Auth.
// The important part about this, is the following:
// 1. Proper setup of the App Registration
// 2. Custom Roles need to Created
// 3. Assignment of Custom Roles to the users
//
// We will leverage these concepts for both the frontend and the backend API.
// Now that we are able to demostrate the concept of getting the claims for a user
// we can do two things:
// 1) The Client can choose to display features that the user has access to based on the roles assigned.
// 2) The Client will pass the token to the API and the API will forbid access to operations the user does not 
//    have access to.
// 

// Configuration
const string ClientId = "e47b4848-430e-42c4-9bcc-e26a16c7faa0";
const string TenantId = "0d8f4519-e72a-4019-8975-59754a33abb1";
const string RedirectUri = "http://localhost";

try
{
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