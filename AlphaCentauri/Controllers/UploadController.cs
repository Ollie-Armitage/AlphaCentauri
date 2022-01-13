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

        // Accumulate the form data key-value pairs in the request (formAccumulator).
        var formAccumulator = new KeyValueAccumulator();
        var trustedFileNameForDisplay = string.Empty;
        var untrustedFileNameForStorage = string.Empty;
        var streamedFileContent = Array.Empty<byte>();

        var boundary = MultipartRequestHelper.GetBoundary(
            MediaTypeHeaderValue.Parse(Request.ContentType),
            _defaultFormOptions.MultipartBoundaryLengthLimit);
        var reader = new MultipartReader(boundary, HttpContext.Request.Body);

        
        var uploadModel = new UploadModel();
        
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
                    uploadModel.Files.Add(await FileHelper.BuildFileModel(contentDisposition, section, ModelState, _appOptions));
                }
                else if (MultipartRequestHelper
                    .HasFormDataContentDisposition(contentDisposition))
                {
                    uploadModel.FormAccumulator = await FormHelper.AddFormData(uploadModel.FormAccumulator, contentDisposition, section,
                        ModelState, _defaultFormOptions.ValueCountLimit);
                }
                
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
            }

            // Drain any remaining section body that hasn't been consumed and
            // read the headers for the next section.
            section = await reader.ReadNextSectionAsync();
        }

        // Bind form data to the model
        var formData = new FormData();
        var formValueProvider = new FormValueProvider(
            BindingSource.Form,
            new FormCollection(uploadModel.FormAccumulator.GetResults()),
            CultureInfo.CurrentCulture);
        var bindingSuccessful = await TryUpdateModelAsync(formData, prefix: "",
            valueProvider: formValueProvider);

        if (!bindingSuccessful)
        {
            ModelState.AddModelError("File", 
                "The request couldn't be processed (Error 5).");
            // Log error

            return BadRequest(ModelState);
        }

        // **WARNING!**
        // In the following example, the file is saved without
        // scanning the file's contents. In most production
        // scenarios, an anti-virus/anti-malware scanner API
        // is used on the file before making the file available
        // for download or for use by other systems. 
        // For more information, see the topic that accompanies 
        // this sample app.

        foreach (var file in uploadModel.Files)
        {
            new AppFile()
            {
                Content = streamedFileContent,
                UntrustedName = untrustedFileNameForStorage,
                Note = formData.Note,
                Size = streamedFileContent.Length, 
                UploadDT = DateTime.UtcNow
            };


            await using (var targetStream = System.IO.File.Create(Path.Combine(_appOptions.QuarantinePath, file.UntrustedName)))
            {
                await targetStream.WriteAsync(file.FileContent);
            }
        }
        


        // _context.File.Add(file);
        // await _context.SaveChangesAsync();

        return Created(nameof(UploadController), null);
    }
}