using AgencyHub.Application.DTOs.Clients;
using AgencyHub.Application.DTOs.Common;
using AgencyHub.Application.Interfaces;

using AgencyHub.Domain.Entities;

using AgencyHub.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

namespace AgencyHub.API;

public class ClientService : IClientService
{
    private readonly ApplicationDbContext _context;

    private readonly ITenantContext _tenantContext;

    public ClientService(
        ApplicationDbContext context,
        ITenantContext tenantContext)
    {
        _context = context;

        _tenantContext = tenantContext;
    }

    // =========================================================
    // GET CLIENTS
    // Tenant Isolation
    // Search
    // Filtering
    // Pagination
    // =========================================================

    public async Task<PagedResponse<Client>>
        GetClientsAsync(
            ClientQueryRequest request)
    {
        EnsureTenant();

        var tenantId =
            _tenantContext.TenantId!.Value;

        // -----------------------------------------------------
        // Pagination validation
        // -----------------------------------------------------

        var page =
            request.Page < 1
                ? 1
                : request.Page;

        var pageSize =
            request.PageSize < 1
                ? 10
                : Math.Min(
                    request.PageSize,
                    100);

        // -----------------------------------------------------
        // Tenant isolation
        // -----------------------------------------------------

        var query =
            _context.Clients
                .AsNoTracking()
                .Where(x =>
                    x.TenantId == tenantId);

        // -----------------------------------------------------
        // Search
        // -----------------------------------------------------

        if (!string.IsNullOrWhiteSpace(
            request.Search))
        {
            var search =
                request.Search.Trim();

            query =
                query.Where(x =>
                    x.CompanyName.Contains(
                        search)

                    ||

                    x.ContactPerson.Contains(
                        search)

                    ||

                    x.Email.Contains(
                        search)

                    ||

                    (
                        x.Industry != null
                        &&
                        x.Industry.Contains(
                            search)
                    ));
        }

        // -----------------------------------------------------
        // Status filter
        // -----------------------------------------------------

        if (!string.IsNullOrWhiteSpace(
            request.Status))
        {
            var status =
                request.Status.Trim();

            query =
                query.Where(x =>
                    x.Status == status);
        }

        // -----------------------------------------------------
        // Industry filter
        // -----------------------------------------------------

        if (!string.IsNullOrWhiteSpace(
            request.Industry))
        {
            var industry =
                request.Industry.Trim();

            query =
                query.Where(x =>
                    x.Industry == industry);
        }

        // -----------------------------------------------------
        // Total count
        // -----------------------------------------------------

        var totalCount =
            await query.CountAsync();

        // -----------------------------------------------------
        // Pagination
        // -----------------------------------------------------

        var items =
            await query
                .OrderByDescending(
                    x => x.CreatedAt)
                .Skip(
                    (page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

        // -----------------------------------------------------
        // Total pages
        // -----------------------------------------------------

        var totalPages =
            (int)Math.Ceiling(
                totalCount /
                (double)pageSize);

        // -----------------------------------------------------
        // Return response
        // -----------------------------------------------------

        return new PagedResponse<Client>
        {
            Items = items,

            Page = page,

            PageSize = pageSize,

            TotalCount = totalCount,

            TotalPages = totalPages
        };
    }

    // =========================================================
    // CREATE CLIENT
    // =========================================================

    public async Task<Client>
        CreateClientAsync(
            CreateClientRequest request)
    {
        EnsureTenant();

        var tenantId =
            _tenantContext.TenantId!.Value;

        // -----------------------------------------------------
        // IMPORTANT:
        // TenantId comes from JWT.
        // Never from frontend.
        // -----------------------------------------------------

        var client = new Client
        {
            TenantId =
                tenantId,

            CompanyName =
                request.CompanyName.Trim(),

            ContactPerson =
                request.ContactPerson.Trim(),

            Email =
                request.Email
                    .Trim()
                    .ToLowerInvariant(),

            Phone =
                request.Phone?.Trim(),

            Address =
                request.Address?.Trim(),

            Industry =
                request.Industry?.Trim(),

            Status =
                request.Status.Trim(),

            Notes =
                request.Notes?.Trim(),

            CreatedAt =
                DateTime.UtcNow,

            UpdatedAt =
                DateTime.UtcNow
        };

        await _context.Clients
            .AddAsync(client);

        await _context.SaveChangesAsync();

        return client;
    }

    // =========================================================
    // SECURITY CHECK
    // =========================================================

    private void EnsureTenant()
    {
        if (!_tenantContext.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "Authentication is required.");
        }

        if (!_tenantContext.TenantId.HasValue)
        {
            throw new UnauthorizedAccessException(
                "Tenant information is missing.");
        }
    }
}