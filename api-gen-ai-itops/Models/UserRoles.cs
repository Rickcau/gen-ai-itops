namespace api_gen_ai_itops.Models
{
    public static class UserRoles
    {
        public const string Admin = "admin";
        public const string User = "user";
        public const string ReadOnly = "readonly";
        public const string Support = "support";

        public static readonly IEnumerable<string> ValidRoles = new[]
        {
           Admin,
           User,
           ReadOnly,
           Support
        };
    }
}
