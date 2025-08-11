using Microsoft.AspNetCore.Mvc;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Controllers;

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