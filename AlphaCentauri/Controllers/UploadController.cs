using Microsoft.AspNetCore.Mvc;

namespace AlphaCentauri.Controllers;


[ApiController]
[Route("[controller]")]
public class UploadController : ControllerBase
{
    public UploadController()
    {
        
    }

    [HttpPost]
    public async Task<IActionResult> Post()
    {
        var request = HttpContext.Request;
        
        
        
        return Ok();
    }
}