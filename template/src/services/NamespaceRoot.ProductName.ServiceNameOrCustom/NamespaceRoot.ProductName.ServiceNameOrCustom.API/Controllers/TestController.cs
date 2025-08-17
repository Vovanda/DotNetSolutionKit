using Microsoft.AspNetCore.Mvc;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Controllers;

/// <summary>
/// Контроллер для тестирования работы API.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    /// <summary>
    /// Простейший тестовый эндпоинт.
    /// </summary>
    /// <returns>Строку "OK"</returns>
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok("OK");
    }
}