using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using WebSearchIndexing.Modules.Reporting.Application.Queries;

namespace WebSearchIndexing.Modules.Reporting.Api.Endpoints;

internal static class StatsEndpoints
{
    public static RouteGroupBuilder MapStatsEndpoints(this RouteGroupBuilder reportingGroup)
    {
        ArgumentNullException.ThrowIfNull(reportingGroup);

        reportingGroup.MapGet("/dashboard", GetDashboardStats)
            .WithName("GetDashboardStats")
            .WithSummary("Get dashboard statistics");

        reportingGroup.MapGet("/period", GetPeriodStats)
            .WithName("GetPeriodStats")
            .WithSummary("Get statistics for a specific period");

        reportingGroup.MapGet("/quota", GetQuotaUsage)
            .WithName("GetQuotaUsage")
            .WithSummary("Get quota usage information");

        return reportingGroup;
    }

    private static async Task<IResult> GetDashboardStats(
        IReportingQueryService reportingQueryService,
        CancellationToken cancellationToken)
    {
        var stats = await reportingQueryService.GetDashboardStatsAsync(cancellationToken);
        return Results.Ok(stats);
    }

    private static async Task<IResult> GetPeriodStats(
        IReportingQueryService reportingQueryService,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        if (from >= to)
        {
            return Results.BadRequest("'From' date must be earlier than 'To' date");
        }

        var stats = await reportingQueryService.GetPeriodStatsAsync(from, to, cancellationToken);
        return Results.Ok(stats);
    }

    private static async Task<IResult> GetQuotaUsage(
        IReportingQueryService reportingQueryService,
        DateTime? date,
        CancellationToken cancellationToken)
    {
        var stats = await reportingQueryService.GetQuotaUsageAsync(date, cancellationToken);
        return Results.Ok(stats);
    }
}
