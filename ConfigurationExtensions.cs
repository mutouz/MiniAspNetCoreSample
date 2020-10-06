using Microsoft.Extensions.Configuration;

namespace MiniAspNetCoreSample
{
    public static class ConfigurationExtensions
    {
        public static string GetAppSetting(this IConfiguration configuration, string key)
        {
            return configuration.GetSection("AppSettings")?[key];
        }
    }
}