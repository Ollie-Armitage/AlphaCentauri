using System.Text;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace AlphaCentauri.Helpers;

public class FormHelper
{
    public static async Task<KeyValueAccumulator> AddFormData(KeyValueAccumulator formAccumulator, ContentDispositionHeaderValue contentDisposition,
        MultipartSection section, ModelStateDictionary modelState, int valueCountLimit)
    {
        var key = HeaderUtilities
            .RemoveQuotes(contentDisposition.Name).Value;
        var encoding = GetEncoding(section);

        if (encoding == null)
        {
            modelState.AddModelError("File", 
                $"The request couldn't be processed (Error 2).");
            return formAccumulator;
        }
        
        using (var streamReader = new StreamReader(
                   section.Body,
                   encoding,
                   detectEncodingFromByteOrderMarks: true,
                   bufferSize: 1024,
                   leaveOpen: true))
        {
            // The value length limit is enforced by 
            // MultipartBodyLengthLimit
            var value = await streamReader.ReadToEndAsync();

            if (string.Equals(value, "undefined", 
                    StringComparison.OrdinalIgnoreCase))
            {
                value = string.Empty;
            }

            formAccumulator.Append(key, value);

            if (formAccumulator.ValueCount > 
                valueCountLimit)
            {
                // Form key count limit of 
                // _defaultFormOptions.ValueCountLimit 
                // is exceeded.
                modelState.AddModelError("File", 
                    $"The request couldn't be processed (Error 3).");
                // Log error
                
            }
        }
        
        return formAccumulator;
    } 
    
    private static Encoding GetEncoding(MultipartSection section)
    {
        var hasMediaTypeHeader = 
            MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType);
        
        if (!hasMediaTypeHeader || Encoding.UTF8.Equals(mediaType.Encoding))
        {
            return Encoding.UTF8;
        }

        return mediaType.Encoding;
    }
}