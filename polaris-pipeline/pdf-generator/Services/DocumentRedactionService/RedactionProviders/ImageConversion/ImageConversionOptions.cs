namespace pdf_generator.Services.DocumentRedactionService.RedactionProvider.ImageConversion
{
  public class ImageConversionOptions
  {
    public const string ConfigKey = "ImageConversion";

    public int Resolution { get; set; }

    public int QualityPercent { get; set; }
  }
}