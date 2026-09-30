using AgencyHub.Application.DTOs.Clients;
using AgencyHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgencyHub.API.Controllers;

[ApiController]
[Route("api/clients")]
[Authorize] // Requires authentication for all endpoints in this controller by default
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    // =========================================================
    // GET CLIENTS
    // Any authenticated user from the tenant can view clients
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ClientQueryRequest request)
    {
        var result = await _clientService.GetClientsAsync(request);
        return Ok(result);
    }

    // =========================================================
    // CREATE CLIENT
    // Restricted: Only users with the 'AgencyAdmin' role can create clients
    // =========================================================

    [HttpPost]
    [Authorize(Roles = "AgencyAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateClientRequest request)
    {
        var client = await _clientService.CreateClientAsync(request);

        return Created(
            $"/api/clients/{client.Id}",
            client);
    }
}