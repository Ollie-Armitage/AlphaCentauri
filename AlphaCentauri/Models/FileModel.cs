namespace AlphaCentauri.Models;

public class FileModel
{
    public string UntrustedName { get; set; }
    public string TrustedName { get; set; }
    public byte[] FileContent { get; set; }
}