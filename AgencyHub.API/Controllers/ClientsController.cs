using AgencyHub.Application.DTOs.Clients;
using AgencyHub.Application.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgencyHub.API.Controllers;

[ApiController]
[Route("api/clients")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(
        IClientService clientService)
    {
        _clientService = clientService;
    }

    // =========================================================
    // GET CLIENTS
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] ClientQueryRequest request)
    {
        var result =
            await _clientService
                .GetClientsAsync(request);

        return Ok(result);
    }

    // =========================================================
    // CREATE CLIENT
    // Only AgencyAdmin
    // =========================================================

    [HttpPost]
    [Authorize(Roles = "AgencyAdmin")]
    public async Task<IActionResult> Create(
        CreateClientRequest request)
    {
        var client =
            await _clientService
                .CreateClientAsync(request);

        return Created(
            $"/api/clients/{client.Id}",
            client);
    }
}