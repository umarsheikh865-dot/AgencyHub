using AgencyHub.Application.Services;
using AgencyHub.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AgencyHub.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ClientsController : ControllerBase
	{
		private readonly IClientService _clientService;

		public ClientsController(ClientsController @clientService) // wait, use IClientService
		{
		}

		public ClientsController(IClientService clientService)
		{
			_clientService = clientService;
		}

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var clients = await _clientService.GetAllClientsAsync();
			return Ok(clients);
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] Client client)
		{
			var createdClient = await _clientService.CreateClientAsync(client);
			return Ok(createdClient);
		}
	}
}