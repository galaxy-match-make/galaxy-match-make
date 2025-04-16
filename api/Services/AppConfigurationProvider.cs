using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Amazon;
using System.Text.Json;

namespace galaxy_match_make.Services
{
    public static class AppConfigurationExtensions
    {
        public static void AddAppConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            
            if (environment.Equals("Production", StringComparison.OrdinalIgnoreCase))
            {
                // Register AWS Secrets Manager configuration provider
                services.AddSingleton<IAppConfigurationProvider, AwsSecretsManagerConfigurationProvider>();
            }
            else
            {
                services.AddSingleton<IAppConfigurationProvider, LocalConfigurationProvider>();
            }
        }
    }
    
    public interface IAppConfigurationProvider
    {
        Task<string> GetSecretAsync(string key);
        Task<T> GetSecretsAsync<T>() where T : class, new();
        Task<IConfiguration> GetConfigurationAsync();
    }
    
    /// <summary>
    /// Configuration provider that uses appsettings.json files for local development
    /// </summary>
    public class LocalConfigurationProvider : IAppConfigurationProvider
    {
        private readonly IConfiguration _configuration;
        
        public LocalConfigurationProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public Task<string> GetSecretAsync(string key)
        {
            return Task.FromResult(_configuration[key]);
        }
        
        public Task<T> GetSecretsAsync<T>() where T : class, new()
        {
            var result = new T();
            _configuration.Bind(result);
            return Task.FromResult(result);
        }
        
        public Task<IConfiguration> GetConfigurationAsync()
        {
            return Task.FromResult(_configuration);
        }
    }

    /// <summary>
    /// Configuration provider that uses AWS Secrets Manager for production environments
    /// </summary>
    public class AwsSecretsManagerConfigurationProvider : IAppConfigurationProvider
    {
        private readonly IConfiguration _configuration;
        private readonly AmazonSecretsManagerClient _secretsManager;
        private readonly string _region;
        private readonly string _secretPrefix;
        private readonly string _environment;

        public AwsSecretsManagerConfigurationProvider(IConfiguration configuration)
        {
            _configuration = configuration;
            _region = _configuration["AWS:Region"] ?? Environment.GetEnvironmentVariable("AWS_REGION") ?? "af-south-1";
            _secretsManager = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(_region));
            _environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            _secretPrefix = Environment.GetEnvironmentVariable("AWS_SECRET_PREFIX") ?? "";
        }

        public async Task<string> GetSecretAsync(string key)
        {
            try
            {
                // Format the key to be compatible with AWS Secrets Manager naming
                // Include unique suffix if available
                string secretKey = key.Replace(":", "/");
                string fullSecretKey = !string.IsNullOrEmpty(_secretPrefix) 
                    ? $"{secretKey}-{_environment.ToLower()}-{_secretPrefix}"
                    : $"{secretKey}-{_environment.ToLower()}";
                
                var request = new GetSecretValueRequest
                {
                    SecretId = fullSecretKey
                };

                var response = await _secretsManager.GetSecretValueAsync(request);
                return response.SecretString;
            }
            catch (ResourceNotFoundException)
            {
                // Try without suffix as a fallback
                try
                {
                    string secretKey = key.Replace(":", "/");
                    string fallbackKey = $"{secretKey}-{_environment.ToLower()}";
                    
                    var fallbackRequest = new GetSecretValueRequest
                    {
                        SecretId = fallbackKey
                    };

                    var fallbackResponse = await _secretsManager.GetSecretValueAsync(fallbackRequest);
                    return fallbackResponse.SecretString;
                }
                catch (Exception)
                {
                    // If the secret is not found in Secrets Manager, fall back to configuration
                    return _configuration[key];
                }
            }
            catch (Exception)
            {
                // For any other exception, fall back to configuration
                return _configuration[key];
            }
        }

        public async Task<T> GetSecretsAsync<T>() where T : class, new()
        {
            try
            {
                // Try to get all secrets as a single JSON object
                string secretName = typeof(T).Name;
                string fullSecretName = !string.IsNullOrEmpty(_secretPrefix)
                    ? $"{secretName}-{_environment.ToLower()}-{_secretPrefix}"
                    : $"{secretName}-{_environment.ToLower()}";
                
                var request = new GetSecretValueRequest
                {
                    SecretId = fullSecretName
                };

                var response = await _secretsManager.GetSecretValueAsync(request);
                if (!string.IsNullOrEmpty(response.SecretString))
                {
                    // Try to deserialize the secret string to the requested type
                    return JsonSerializer.Deserialize<T>(response.SecretString);
                }
            }
            catch
            {
                // If getting the secret as a whole object fails, we'll fall back
            }
            
            // Fall back to configuration binding
            var result = new T();
            _configuration.Bind(result);
            return result;
        }
        
        public Task<IConfiguration> GetConfigurationAsync()
        {
            return Task.FromResult(_configuration);
        }
    }
}