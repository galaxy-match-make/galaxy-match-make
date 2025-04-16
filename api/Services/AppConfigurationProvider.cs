using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace galaxy_match_make.Services
{
    public static class AppConfigurationExtensions
    {
        public static IServiceCollection AddSecureConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            // Determine environment
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var useParameterStore = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("USE_PARAMETER_STORE"));
            
            // Register the configuration provider
            services.AddSingleton<IAppConfigurationProvider>(provider => 
            {
                // Choose appropriate provider based on environment and configuration
                if (useParameterStore)
                {
                    var logger = provider.GetService<ILogger<ParameterStoreConfigurationProvider>>();
                    var parameterStoreService = provider.GetService<ParameterStoreService>();
                    return new ParameterStoreConfigurationProvider(parameterStoreService, environment, logger);
                }
                else
                {
                    // For local development, use configuration from appsettings files
                    return new LocalConfigurationProvider(configuration);
                }
            });
            
            // Register required services
            services.AddHttpClient();
            services.AddSingleton<ParameterStoreService>();
            
            return services;
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
    /// Configuration provider that uses AWS Parameter Store for production environments
    /// </summary>
    public class ParameterStoreConfigurationProvider : IAppConfigurationProvider
    {
        private readonly ParameterStoreService _parameterStoreService;
        private readonly ILogger<ParameterStoreConfigurationProvider> _logger;
        private Dictionary<string, string> _cachedParameters;
        private readonly string _environment;
        private readonly IConfiguration _fallbackConfiguration;
        
        public ParameterStoreConfigurationProvider(
            ParameterStoreService parameterStoreService, 
            string environment,
            ILogger<ParameterStoreConfigurationProvider> logger = null)
        {
            _parameterStoreService = parameterStoreService;
            _environment = environment?.ToLower() ?? "development";
            _cachedParameters = new Dictionary<string, string>();
            _logger = logger;
        }
        
        public async Task<string> GetSecretAsync(string key)
        {
            if (_cachedParameters.Count == 0)
            {
                await LoadParametersAsync();
            }
            
            // Try to find the key in the parameters
            string normalizedKey = key.Replace(":", "/");
            foreach (var paramKey in _cachedParameters.Keys)
            {
                if (paramKey.EndsWith(normalizedKey, StringComparison.OrdinalIgnoreCase))
                {
                    return _cachedParameters[paramKey];
                }
            }
            
            // If not found, return null
            return null;
        }
        
        public async Task<T> GetSecretsAsync<T>() where T : class, new()
        {
            if (_cachedParameters.Count == 0)
            {
                await LoadParametersAsync();
            }
            
            // Create a new instance and populate its properties from the parameters
            var result = new T();
            var properties = typeof(T).GetProperties();
            
            foreach (var prop in properties)
            {
                string propPath = prop.Name.Replace(":", "/");
                string matchingKey = null;
                
                // Find a matching key
                foreach (var paramKey in _cachedParameters.Keys)
                {
                    if (paramKey.EndsWith("/" + propPath, StringComparison.OrdinalIgnoreCase))
                    {
                        matchingKey = paramKey;
                        break;
                    }
                }
                
                if (matchingKey != null)
                {
                    try
                    {
                        prop.SetValue(result, Convert.ChangeType(_cachedParameters[matchingKey], prop.PropertyType));
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, $"Failed to set property {prop.Name} with value {_cachedParameters[matchingKey]}");
                    }
                }
            }
            
            return result;
        }
        
        public async Task<IConfiguration> GetConfigurationAsync()
        {
            if (_cachedParameters.Count == 0)
            {
                await LoadParametersAsync();
            }
            
            var configBuilder = new ConfigurationBuilder();
            
            // Normalize parameters to configuration format
            var configParams = new Dictionary<string, string>();
            foreach (var param in _cachedParameters)
            {
                // Transform AWS Parameter Store path format to configuration key format
                // e.g., /galaxy-match/development/Google/ClientId -> Google:ClientId
                var key = param.Key;
                var segments = key.Split('/');
                if (segments.Length >= 3) // Remove the prefix segments (/galaxy-match/environment)
                {
                    key = string.Join(":", segments.Skip(3));
                }
                configParams[key] = param.Value;
            }
            
            configBuilder.AddInMemoryCollection(configParams);
            return configBuilder.Build();
        }
        
        private async Task LoadParametersAsync()
        {
            try
            {
                _cachedParameters = await _parameterStoreService.GetParametersAsync(_environment);
                _logger?.LogInformation($"Loaded {_cachedParameters.Count} parameters from Parameter Store for environment: {_environment}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Failed to load parameters from Parameter Store for environment: {_environment}");
                _cachedParameters = new Dictionary<string, string>();
            }
        }
    }
}