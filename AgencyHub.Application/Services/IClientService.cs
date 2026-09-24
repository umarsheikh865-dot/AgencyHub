using AgencyHub.Domain.Entities;

namespace AgencyHub.Application.Services
{
    public interface IClientService
    {
        Task<IEnumerable<Client>> GetAllClientsAsync();
        Task<Client> CreateClientAsync(Client client);
    }
}