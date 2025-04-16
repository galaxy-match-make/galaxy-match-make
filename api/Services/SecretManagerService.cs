
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System.Text.Json;
using Amazon;

namespace galaxy_match_make.Services;
public class SecretManagerService
{
    private readonly IAmazonSecretsManager _secretsManager;
    private readonly string _secretName = "CSharpLevelUpDbSecret";
    private readonly RegionEndpoint _regionEndpoint = RegionEndpoint.AFSouth1; // Use RegionEndpoint directly

    public SecretManagerService()
    {
        _secretsManager = new AmazonSecretsManagerClient(_regionEndpoint);
    }

    public async Task<T?> GetSecretAsync<T>() where T : class
    {
        try
        {
            var request = new GetSecretValueRequest
            {
                SecretId = _secretName
            };

            var response = await _secretsManager.GetSecretValueAsync(request);

            if (response?.SecretString != null)
            {
                return JsonSerializer.Deserialize<T>(response.SecretString);
            }
            else
            {
                Console.WriteLine($"Warning: Secret '{_secretName}' not found or empty.");
                return null;
            }
        }
        catch (AmazonSecretsManagerException ex)
        {
            Console.WriteLine($"Error retrieving secret '{_secretName}': {ex.Message}");
            return null;
        }
        finally
        {
            _secretsManager.Dispose();
        }
    }
}