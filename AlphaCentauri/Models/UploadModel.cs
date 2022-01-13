using Microsoft.AspNetCore.WebUtilities;

namespace AlphaCentauri.Models;

public class UploadModel
{
    public List<FileModel> Files { get; set; } = new List<FileModel>();

    public KeyValueAccumulator FormAccumulator { get; set; } = new KeyValueAccumulator();
}