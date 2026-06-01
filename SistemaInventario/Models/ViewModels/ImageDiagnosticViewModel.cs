namespace SistemaInventario.Models.ViewModels;

public class ImageDiagnosticViewModel
{
    public int TotalConImagen    { get; set; }
    public int TotalFaltantes    { get; set; }
    public int TotalSinImagen    { get; set; }
    public List<MissingImageItem> ImagenesFaltantes { get; set; } = new();
}

public class MissingImageItem
{
    public int     Id       { get; set; }
    public string  Name     { get; set; } = "";
    public string? ImageUrl { get; set; }
}
