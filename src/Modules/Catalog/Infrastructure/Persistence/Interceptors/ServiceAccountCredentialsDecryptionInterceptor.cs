using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.DataProtection;
using WebSearchIndexing.Modules.Catalog.Domain.Entities;

namespace WebSearchIndexing.Modules.Catalog.Infrastructure.Persistence.Interceptors;

internal sealed class ServiceAccountCredentialsDecryptionInterceptor : IMaterializationInterceptor
{
    private readonly IDataProtector _protector;
    private readonly ILogger<ServiceAccountCredentialsDecryptionInterceptor> _logger;

    public ServiceAccountCredentialsDecryptionInterceptor(
    IDataProtectionProvider dataProtectionProvider,
    ILogger<ServiceAccountCredentialsDecryptionInterceptor> logger)
    {
        _protector = dataProtectionProvider.CreateProtector("Catalog.ServiceAccount.Credentials");
        _logger = logger;
    }

    public object InitializedInstance(MaterializationInterceptionData materializationData, object entity)
    {
        if (entity is ServiceAccount account)
        {
            try
            {
                if (!string.IsNullOrEmpty(account.CredentialsJson))
                {
                    account.UpdateCredentials(SafeUnprotect(account.CredentialsJson));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to unprotect service account credentials during materialization");
            }
        }
        return entity;
    }

    private string SafeUnprotect(string value)
    {
        try
        {
            return _protector.Unprotect(value);
        }
        catch
        {
            // if value is not protected (legacy/plain), just return as is
            return value;
        }
    }
}
