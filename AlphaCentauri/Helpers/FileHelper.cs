using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace AlphaCentauri.Helpers;

public class FileHelper
{
    public static async Task<byte[]> ProcessStreamedFile(MultipartSection section, ContentDispositionHeaderValue contentDisposition, ModelStateDictionary modelState, object permittedExtensions, object fileSizeLimit)
    {
        throw new NotImplementedException();
    }
}