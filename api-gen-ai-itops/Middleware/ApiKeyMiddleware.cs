namespace api_gen_ai_itops.Middleware
{
    public class ApiKeyMiddleware
    {
        private const string API_KEY_NAME = "api-key";
        private readonly string _apiKey;
        private readonly RequestDelegate _next;

        private readonly List<string> _includedPaths = new List<string>
        {
            "/Chat",
            "/Chat/patch-missing-person"
        };

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _apiKey = configuration.GetValue<string>("MissingPersonApiKey") ?? throw new ArgumentNullException("MissingPersonApiKey");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (_includedPaths.Any(path => context.Request.Path.StartsWithSegments(path)))
            {
                if (!context.Request.Headers.TryGetValue(API_KEY_NAME, out var extractedApiKey))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("API Key is missing or incorrect.");
                    return;
                }

                if (!_apiKey.Equals(extractedApiKey))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Unauthorized client.");
                    return;
                }
            }

            await _next(context);
        }
    }
}