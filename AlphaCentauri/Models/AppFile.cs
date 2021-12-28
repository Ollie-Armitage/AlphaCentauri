namespace AlphaCentauri.Models;

public class AppFile
{
    public byte[] Content { get; set; }
    public string UntrustedName { get; set; }
    public object Note { get; set; }
    public int Size { get; set; }
    public DateTime UploadDT { get; set; }
}