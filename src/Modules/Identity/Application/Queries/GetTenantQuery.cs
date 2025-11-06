using MediatR;
using WebSearchIndexing.Modules.Identity.Application.DTOs;

namespace WebSearchIndexing.Modules.Identity.Application.Queries;

public record GetTenantQuery(Guid TenantId) : IRequest<TenantDto?>;
