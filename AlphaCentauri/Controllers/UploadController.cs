using System.Globalization;
using System.Net;
using System.Text;
using AlphaCentauri.Helpers;
using AlphaCentauri.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace AlphaCentauri.Controllers;


[ApiController]
[Route("[controller]")]
public class UploadController : ControllerBase
{
    private readonly FormOptions _defaultFormOptions;
    private readonly AppOptions _appOptions;

    public UploadController(IOptions<FormOptions> defaultFormOptions, IOptions<AppOptions> appOptions)
    {
        _defaultFormOptions = defaultFormOptions.Value;
        _appOptions = appOptions.Value;
    }

    [HttpPost]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> Post()
    {
        if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
        {
            ModelState.AddModelError("File", 
                $"The request couldn't be processed (Error 1).");
            // Log error

            return BadRequest(ModelState);
        }

        var boundary = MultipartRequestHelper.GetBoundary(
            MediaTypeHeaderValue.Parse(Request.ContentType),
            _defaultFormOptions.MultipartBoundaryLengthLimit);

        List<FileModel> files = new List<FileModel>(); 
        
        var reader = new MultipartReader(boundary, HttpContext.Request.Body);
        var section = await reader.ReadNextSectionAsync();
        while (section != null)
        {
            var hasContentDispositionHeader = 
                ContentDispositionHeaderValue.TryParse(
                    section.ContentDisposition, out var contentDisposition);
            
            if (hasContentDispositionHeader)
            {
                if (MultipartRequestHelper
                    .HasFileContentDisposition(contentDisposition))
                {
                    files.Add(await FileHelper.BuildFileModel(contentDisposition, section, ModelState, _appOptions));
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
            }
            
            section = await reader.ReadNextSectionAsync();
        }

        foreach (var file in files)
        {
            await using var targetStream = System.IO.File.Create(Path.Combine(_appOptions.QuarantinePath, file.UntrustedName));
            await targetStream.WriteAsync(file.FileContent);
        }

        return Created(nameof(UploadController), null);
    }
}