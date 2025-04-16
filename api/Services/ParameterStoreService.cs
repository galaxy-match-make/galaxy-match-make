using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Microsoft.Extensions.Configuration;

namespace galaxy_match_make.Services
{
    public class ParameterStoreService
    {
        private readonly IAmazonSimpleSystemsManagement _ssmClient;
        private readonly string _parameterPrefix;
        
        public ParameterStoreService(IConfiguration configuration)
        {
            var region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "af-south-1";
            _ssmClient = new AmazonSimpleSystemsManagementClient(RegionEndpoint.GetBySystemName(region));
            _parameterPrefix = Environment.GetEnvironmentVariable("PARAMETER_PREFIX") ?? "/galaxy-match/";
        }
        
        public async Task<Dictionary<string, string>> GetParametersAsync(string environment)
        {
            var path = $"{_parameterPrefix}{environment}/";
            var parameters = new Dictionary<string, string>();
            
            try
            {
                var request = new GetParametersByPathRequest
                {
                    Path = path,
                    Recursive = true,
                    WithDecryption = true
                };
                
                var response = await _ssmClient.GetParametersByPathAsync(request);
                
                foreach (var parameter in response.Parameters)
                {
                    // Extract the parameter name without the prefix path
                    var paramName = parameter.Name.Replace(path, "").Replace("/", ":");
                    parameters[paramName] = parameter.Value;
                }
                
                return parameters;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving parameters: {ex.Message}");
                return parameters;
            }
        }
    }
}