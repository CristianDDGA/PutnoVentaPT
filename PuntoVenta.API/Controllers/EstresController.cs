using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PuntoVenta.Application.Constants;
using PuntoVenta.Infrastructure.Persistence;

namespace PuntoVenta.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = AppRoles.Admin)]
public class EstresController : ControllerBase
{
    private readonly DataSeedingService _seedingService;

    public EstresController(DataSeedingService seedingService)
    {
        _seedingService = seedingService;
    }

    [HttpPost("cargar-100")]
    public async Task<IActionResult> CargarDatos()
    {
        try
        {
            await _seedingService.GenerarDatosEstresAsync(100);
            return Ok("¡Éxito! Se han insertado 100 registros en cada tabla en SQL Server de forma limpia y óptima.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error en la carga: {ex.Message}");
        }
    }
}