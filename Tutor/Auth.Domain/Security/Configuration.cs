namespace Auth.Domain.Security;

public class Configuration
{
    public static SecretsConfiguration Secrets { get; set; } = new();
    public class SecretsConfiguration
    {
        public string ApiKey { get; set; } = string.Empty;
        public string JwtPrivateKey { get; set; } = string.Empty;
        public string PasswordSaltKey { get; set; } = string.Empty;
    }
}