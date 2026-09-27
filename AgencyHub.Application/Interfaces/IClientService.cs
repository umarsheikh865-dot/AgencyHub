using AgencyHub.Application.DTOs.Clients;
using AgencyHub.Application.DTOs.Common;

using AgencyHub.Domain.Entities;

namespace AgencyHub.Application.Interfaces;

public interface IClientService
{
    Task<PagedResponse<Client>> GetClientsAsync(
        ClientQueryRequest request);

    Task<Client> CreateClientAsync(
        CreateClientRequest request);
}