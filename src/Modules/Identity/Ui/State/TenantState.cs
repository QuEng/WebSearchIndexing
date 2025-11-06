using WebSearchIndexing.Modules.Identity.Ui.Models;

namespace WebSearchIndexing.Modules.Identity.Ui.State;

public class TenantState
{
    public Guid? SelectedTenantId { get; set; }
    public string SelectedTenantName { get; set; } = string.Empty;
    public string SelectedTenantSlug { get; set; } = string.Empty;
    public string UserRoleInTenant { get; set; } = string.Empty;
    public List<TenantInfo> AvailableTenants { get; set; } = new();
    
    public bool HasSelectedTenant => SelectedTenantId.HasValue;
    public TenantInfo? CurrentTenant => AvailableTenants.FirstOrDefault(t => t.Id == SelectedTenantId);
    
    public void SelectTenant(TenantInfo tenant)
    {
        SelectedTenantId = tenant.Id;
        SelectedTenantName = tenant.Name;
        SelectedTenantSlug = tenant.Slug;
        UserRoleInTenant = tenant.Role;
    }
    
    public void UpdateAvailableTenants(List<TenantInfo> tenants)
    {
        AvailableTenants = tenants.ToList();
        
        // If current selection is no longer available, clear it
        if (SelectedTenantId.HasValue && !tenants.Any(t => t.Id == SelectedTenantId))
        {
            ClearSelection();
        }
    }
    
    public void ClearSelection()
    {
        SelectedTenantId = null;
        SelectedTenantName = string.Empty;
        SelectedTenantSlug = string.Empty;
        UserRoleInTenant = string.Empty;
    }
    
    public void Clear()
    {
        ClearSelection();
        AvailableTenants.Clear();
    }
}
