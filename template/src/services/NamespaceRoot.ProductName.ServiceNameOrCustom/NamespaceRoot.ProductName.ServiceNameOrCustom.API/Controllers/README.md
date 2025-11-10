# Controller Development Guide

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NamespaceRoot.ProductName.Common.Application.Authorization;
using NamespaceRoot.ProductName.Common.Web.Authorization;
using NamespaceRoot.ProductName.ServiceNameOrCustom.Application.Services.Sample;
using NamespaceRoot.ProductName.ServiceNameOrCustom.Common.Contracts.Sample;

namespace NamespaceRoot.ProductName.ServiceNameOrCustom.API.Controllers;

/// <summary>
/// Sample controller for managing samples
/// </summary>
[ApiController]
[Authorize]                                    
[Route(SampleControllerRoutes.SampleControllerRoot)] // ✅ MUST use route constants with version
public class SampleController : ControllerBase
{
    private readonly ISampleService _sampleService;

    /// <summary>
    /// Initializes a new instance of the SampleController
    /// </summary>
    public SampleController(ISampleService sampleService)
    {
        _sampleService = sampleService;
    }

    /// <summary>
    /// Retrieves all samples in the system
    /// </summary>
    /// <param name="filter">Filter criteria for samples</param>
    [HttpGet(SampleControllerRoutes.GetAllSamples)] // ✅ Route constant
    [RequiredPermissions(AppPermissions.SamplePermission.View)] 
    [ProducesResponseType(StatusCodes.Status200OK)] 
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<GetAllSamplesResponse> GetAllSamples([FromQuery] SampleFilter filter)
    {
        return _sampleService.GetAllSamplesAsync(filter, HttpContext.RequestAborted); // ✅ No async/await
    }

    /// <summary>
    /// Retrieves a specific sample by ID
    /// </summary>
    /// <param name="id">Sample identifier</param>
    [HttpGet(SampleControllerRoutes.GetSampleById)] 
    [RequiredPermissions(AppPermissions.SamplePermission.View)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<GetSampleResponse> GetSampleById([FromRoute] Guid id)
    {
        return _sampleService.GetSampleByIdAsync(id, HttpContext.RequestAborted); // ✅ Direct Task return
    }

    /// <summary>
    /// Creates a new sample
    /// </summary>
    /// <param name="request">Sample creation data</param>
    [HttpPost(SampleControllerRoutes.CreateSample)] 
    [RequiredPermissions(AppPermissions.SamplePermission.Create)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)] // ✅ 201 for POST
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public Task<CreateSampleResponse> CreateSample([FromBody] CreateSampleRequest request)
    {
        return _sampleService.CreateSampleAsync(request, HttpContext.RequestAborted); // ✅ Specific response type
    }

    /// <summary>
    /// Updates an existing sample
    /// </summary>
    /// <param name="id">Sample identifier</param>
    /// <param name="request">Sample update data</param>
    [HttpPut(SampleControllerRoutes.UpdateSample)]
    [RequiredPermissions(AppPermissions.SamplePermission.Edit)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public Task<UpdateSampleResponse> UpdateSample(
        [FromRoute] Guid id, 
        [FromBody] UpdateSampleRequest request)
    {
        return _sampleService.UpdateSampleAsync(id, request, HttpContext.RequestAborted);
    }

    /// <summary>
    /// Deletes a sample
    /// </summary>
    /// <param name="id">Sample identifier</param>
    [HttpDelete(SampleControllerRoutes.DeleteSample)]
    [RequiredPermissions(AppPermissions.SamplePermission.Delete)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<DeleteSampleResponse> DeleteSample([FromRoute] Guid id)
    {
        return _sampleService.DeleteSampleAsync(id, HttpContext.RequestAborted);
    }
}
```

## Route Constants (Required)

```csharp
namespace NamespaceRoot.ProductName.ServiceNameOrCustom.Common.Contracts.Sample;

/// <summary>
/// Sample controller API endpoints
/// </summary>
public static class SampleControllerRoutes
{
    private const string ApiWithVersion = "/api/v1/";

    /// <summary>Sample controller root</summary>
    public const string SampleControllerRoot = ApiWithVersion + "samples";
    
    /// <summary>Get all samples</summary>
    public const string GetAllSamples = "";
    
    /// <summary>Get sample by ID</summary>
    public const string GetSampleById = "{id}";
    
    /// <summary>Create sample</summary>
    public const string CreateSample = "";
    
    /// <summary>Update sample</summary>
    public const string UpdateSample = "{id}";
    
    /// <summary>Delete sample</summary>
    public const string DeleteSample = "{id}";
}
```

## Key Rules Summary

1. **Route Constants**: ✅ Always use `*ControllerRoutes` with version in path
2. **No async/await**: ✅ Return `Task<T>` directly from service calls
3. **Specific Response Types**: ✅ No `IActionResult`, return concrete types
4. **Response Envelope**: ✅ Use Automatic wrapping by filter, no manual `ApiResponse<T>`
5. **XML Documentation**: ✅ Swagger summaries and param docs for all methods
6. **Response Status Codes**: ✅ Appropriate codes for each operation (201 POST, 404 GET, etc.)
7. **Error Handling**: ✅ Global middleware, no try-catch in controllers
8. **Route Attributes**: ✅ Use route constants in `[HttpGet]`, `[HttpPost]`, etc.
9. **Versioning**: ✅ Version included in route constants (`/api/v1/`)