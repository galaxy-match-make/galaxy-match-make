namespace galaxy_match_make.Services;

using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

public class DatabaseService
{
    private readonly IAmazonSecretsManager _secretsManager;
    private readonly string _secretName = "CSharpLevelUpDbSecret";
    private readonly string _regionEndpoint = "af-south-1"; // Ensure this matches

    public DatabaseService(IAmazonSecretsManager secretsManager)
    {
        _secretsManager = secretsManager;
    }

    public async Task<DatabaseCredentials> GetDatabaseCredentialsAsync()
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
                // Assuming your secret is stored as a JSON string
                var credentials = System.Text.Json.JsonSerializer.Deserialize<DatabaseCredentials>(response.SecretString);
                return credentials;
            }
            else
            {
                // Handle the case where the secret is not found or empty
                throw new Exception($"Secret '{_secretName}' not found or empty.");
            }
        }
        catch (AmazonSecretsManagerException ex)
        {
            // Handle AWS Secrets Manager specific exceptions (e.g., access denied)
            Console.WriteLine($"Error retrieving secret: {ex.Message}");
            throw;
        }
    }
}

public class DatabaseCredentials
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Host { get; set; }
    // Add other relevant properties
}