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

    // =========================================================
    // UPDATE CLIENT
    // Restricted: Only users with the 'AgencyAdmin' role can update clients
    // =========================================================

    [HttpPut("{id}")]
    [Authorize(Roles = "AgencyAdmin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClientRequest request)
    {
        var updatedClient = await _clientService.UpdateClientAsync(id, request);
        if (updatedClient == null)
        {
            return NotFound(new { message = "Client not found." });
        }

        return Ok(updatedClient);
    }

    // =========================================================
    // DELETE CLIENT
    // Restricted: Only users with the 'AgencyAdmin' role can delete clients
    // =========================================================

    [HttpDelete("{id}")]
    [Authorize(Roles = "AgencyAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _clientService.DeleteClientAsync(id);
        if (!success)
        {
            return NotFound(new { message = "Client not found." });
        }

        return NoContent();
    }
}