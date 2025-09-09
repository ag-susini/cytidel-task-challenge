using Microsoft.AspNetCore.Mvc;

namespace Tasker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected ActionResult<T> OkOrNotFound<T>(T? result)
    {
        if (result == null)
            return NotFound();
        
        return Ok(result);
    }
}