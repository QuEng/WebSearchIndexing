using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.DataProtection;
using System.Data.Common;
using System.Threading;
using WebSearchIndexing.Modules.Catalog.Domain.Entities;

namespace WebSearchIndexing.Modules.Catalog.Infrastructure.Persistence.Interceptors;

internal sealed class ServiceAccountCredentialsEncryptionInterceptor : SaveChangesInterceptor
{
    private readonly IDataProtector _protector;
    private readonly ILogger<ServiceAccountCredentialsEncryptionInterceptor> _logger;

    public ServiceAccountCredentialsEncryptionInterceptor(
    IDataProtectionProvider dataProtectionProvider,
    ILogger<ServiceAccountCredentialsEncryptionInterceptor> logger)
    {
        _protector = dataProtectionProvider.CreateProtector("Catalog.ServiceAccount.Credentials");
        _logger = logger;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        ProtectSensitiveFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        ProtectSensitiveFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void ProtectSensitiveFields(DbContext? context)
    {
        if (context is null) return;

        var entries = context.ChangeTracker.Entries<ServiceAccount>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                try
                {
                    var json = entry.Entity.CredentialsJson;
                    if (!string.IsNullOrEmpty(json) && !IsProtected(json))
                    {
                        entry.Entity.UpdateCredentials(_protector.Protect(json));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to protect service account credentials");
                    throw;
                }
            }
        }
    }

    private bool IsProtected(string value)
    {
        try
        {
            _ = _protector.Unprotect(value);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
