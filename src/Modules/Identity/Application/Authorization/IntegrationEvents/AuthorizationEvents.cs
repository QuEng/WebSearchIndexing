using WebSearchIndexing.BuildingBlocks.Messaging;
using WebSearchIndexing.Modules.Identity.Domain.Entities;

namespace WebSearchIndexing.Modules.Identity.Application.Authorization.IntegrationEvents;

public sealed class RoleAssignedEvent : IntegrationEvent
{
    public Guid UserId { get; }
    public string Role { get; }
    public RoleType RoleType { get; }
    public Guid? TenantIdValue { get; }
    public Guid? DomainId { get; }

    public RoleAssignedEvent(
        string tenantId,
        Guid userId,
        string role,
        RoleType roleType,
        Guid? tenantIdValue = null,
        Guid? domainId = null) : base(tenantId)
    {
        UserId = userId;
        Role = role;
        RoleType = roleType;
        TenantIdValue = tenantIdValue;
        DomainId = domainId;
    }
}

public sealed class UserRegisteredEvent : IntegrationEvent
{
    public Guid UserId { get; }
    public string Email { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public DateTime RegisteredAt { get; }

    public UserRegisteredEvent(
        string tenantId,
        Guid userId,
        string email,
        string firstName,
        string lastName,
        DateTime registeredAt) : base(tenantId)
    {
        UserId = userId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        RegisteredAt = registeredAt;
    }
}

public sealed class TenantCreatedEvent : IntegrationEvent
{
    public Guid TenantIdValue { get; }
    public string TenantName { get; }
    public string TenantSlug { get; }
    public Guid CreatedByUserId { get; }
    public DateTime CreatedAt { get; }

    public TenantCreatedEvent(
        string tenantId,
        Guid tenantIdValue,
        string tenantName,
        string tenantSlug,
        Guid createdByUserId,
        DateTime createdAt) : base(tenantId)
    {
        TenantIdValue = tenantIdValue;
        TenantName = tenantName;
        TenantSlug = tenantSlug;
        CreatedByUserId = createdByUserId;
        CreatedAt = createdAt;
    }
}

public sealed class TokenRefreshedEvent : IntegrationEvent
{
    public Guid UserId { get; }
    public DateTime RefreshedAt { get; }
    public string ClientInfo { get; }
    public bool WasRotated { get; }

    public TokenRefreshedEvent(
        string tenantId,
        Guid userId,
        DateTime refreshedAt,
        string clientInfo,
        bool wasRotated) : base(tenantId)
    {
        UserId = userId;
        RefreshedAt = refreshedAt;
        ClientInfo = clientInfo;
        WasRotated = wasRotated;
    }
}

public sealed class SecurityEvent : IntegrationEvent
{
    public Guid UserId { get; }
    public string EventType { get; }
    public string EventDetails { get; }
    public DateTime OccurredAt { get; }
    public string IpAddress { get; }
    public string UserAgent { get; }

    public SecurityEvent(
        string tenantId,
        Guid userId,
        string eventType,
        string eventDetails,
        DateTime occurredAt,
        string ipAddress,
        string userAgent) : base(tenantId)
    {
        UserId = userId;
        EventType = eventType;
        EventDetails = eventDetails;
        OccurredAt = occurredAt;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }
}

public sealed class UserInvitedEvent : IntegrationEvent
{
    public Guid InvitationId { get; }
    public string Email { get; }
    public string Role { get; }
    public Guid InvitedByUserId { get; }
    public string OrganizationName { get; }
    public DateTime InvitedAt { get; }

    public UserInvitedEvent(
        string tenantId,
        Guid invitationId,
        string email,
        string role,
        Guid invitedByUserId,
        string organizationName,
        DateTime invitedAt) : base(tenantId)
    {
        InvitationId = invitationId;
        Email = email;
        Role = role;
        InvitedByUserId = invitedByUserId;
        OrganizationName = organizationName;
        InvitedAt = invitedAt;
    }
}
