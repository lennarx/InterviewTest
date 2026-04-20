using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace DotNetExamApi.Infrastructure;

public class KeyVaultService
{
    private readonly SecretClient _secretClient;

    public KeyVaultService(IConfiguration configuration)
    {
        var vaultUri = configuration["KeyVault:VaultUri"]
            ?? throw new InvalidOperationException("KeyVault:VaultUri is not configured.");

        var tenantId = configuration["KeyVault:TenantId"];
        var clientId = configuration["KeyVault:ClientId"];
        var clientSecret = configuration["KeyVault:ClientSecret"];

        var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
        _secretClient = new SecretClient(new Uri(vaultUri), credential);
    }

    public async Task<string?> GetSecretAsync(string secretName)
    {
        var response = await _secretClient.GetSecretAsync(secretName).ConfigureAwait(false);
        return response.Value.Value;
    }

    public async Task SetSecretAsync(string secretName, string value)
    {
        await _secretClient.SetSecretAsync(secretName, value).ConfigureAwait(false);
    }

    public async Task<string?> GetConnectionStringAsync(string name)
    {
        return await GetSecretAsync($"ConnectionStrings--{name}").ConfigureAwait(false);
    }
}
