namespace AlphaCentauri.Models;

public class AppOptions
{
    public int FileSizeLimit { get; set; } = 1024 * 1024 * 1000;
    
    public string QuarantinePath { get; set; } ="D:\\quarantine";

    public string[] PermittedExtensions { get; set; } = { ".doc", ".docx", ".jpg", ".pdf" };
}