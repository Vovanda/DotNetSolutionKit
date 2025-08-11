using Microsoft.AspNetCore.Mvc;

namespace Sawking.Avox.KnowlegeService.API.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    public TestController()
    {
    }

    [HttpGet(Name = "Ok")]
    public OkResult GetOk()
    {
        return Ok();
    }
}