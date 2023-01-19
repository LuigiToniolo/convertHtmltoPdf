using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Web;
using Wkhtmltopdf.NetCore;

namespace AWSServerless2.Controllers;

[Route("api/ExportPdf")]
public class ExportPdfController : ControllerBase
{
    private readonly ILogger<ExportPdfController> _logger;
    readonly IGeneratePdf _generatePdf;

    public ExportPdfController(ILogger<ExportPdfController> logger, IGeneratePdf generatePdf)
    {
        _logger = logger;
        _generatePdf = generatePdf;
        //#if DEBUG
        //        //windows
        //        string filePath = $@"{System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\Rotativa\Windows\libwkhtmltox";
        //#else
        //        //linux
        //        string filePath = @$"{(($@"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Rotativa/Linux/wkhtmltopdf").Replace(@"\", @"/"))}";
        //#endif
        //        //var path = filePath.Replace("libwkhtmltox.so", "").Replace("libwkhtmltox.dll", "").Replace("libwkhtmltox", "").Replace("libwkhtmltox", "");
    }

    [HttpGet("Teste/{id}")]
    public async Task<IActionResult> Teste(int id)
    {
        var pdf = await Test();
        this.HttpContext.Response.Headers.TryAdd("Content-Type", "application/json");
        return new FileContentResult(pdf, "application/pdf");
    }

    private async Task<T> GetResource<T>(string name)
    {
        var ass = this.GetType().Assembly;
        var resources = ass.GetManifestResourceNames();
        var resourceName = resources.FirstOrDefault(a => a.Contains(name));
        using var stream = ass.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();
        if (typeof(T) == typeof(string))
        {
            return (T)((object)content);
        }
        var serialized = System.Text.Json.JsonSerializer.Deserialize<T>(content);
        return serialized;
    }

    private async Task<byte[]> Test()
    {
        _logger.LogInformation($"Test: Fase: 1");
        var html = await GetResource<string>("templateWeeklySupplyByPriority");

        var bytes = await ConverterHtmlToPdf(html);
        return bytes;
        //_logger.LogInformation($"Test: Fase: X1: '{bytes.Length}'");
        //var base64 = EncodeToBase64(bytes);
        //_logger.LogInformation($"Test: Fase: X2: '{base64.Length}'");
        //return base64;
    }

    //static public string EncodeToBase64(byte[] byteArray)
    //{
    //    try
    //    {
    //        string resultado = Convert.ToBase64String(byteArray);
    //        return resultado;
    //    }
    //    catch (Exception)
    //    {
    //        throw;
    //    }
    //}

    private async Task<byte[]> ConverterHtmlToPdf(string html)
    {
        try
        {

            //fonte: https://unicode-table.com/en/html-entities/
            //TODO: ao invés de pegar somente um determinado trecho de <script>
            //devemos pegar todos os trechos de <script> do html e fazer o replace das letras com acento
            //fazer split por <script> vc terá algo assim: ["<html><head><head><body><script>","var xpto = 'js'; </script>"]

            var startTag = "<script type=\"text/javascript\" charset=\"UTF-8\">";
            if (html.Contains(startTag))
            {
                var unicodes = await GetResource<IEnumerable<UnicodeCharsItem>>("UnicodeChars");
                var htmlAteScript = html.Substring(0, html.IndexOf(startTag));
                var htmlScript = html.Substring(htmlAteScript.Length);
                foreach (var unicode in unicodes)
                {
                    htmlScript = htmlScript.Replace(unicode.Char, unicode.Unicode);

                }

                var htmlPronto = htmlAteScript + htmlScript;
                html = htmlPronto;
            }

            ////#######################
            //Encoding utf8 = Encoding.UTF8;
            //byte[] utfBytes = utf8.GetBytes(html);

            //Encoding iso = Encoding.ASCII;
            //byte[] isoBytes = Encoding.Convert(utf8, iso, utfBytes);
            //html = iso.GetString(isoBytes);
            ////#######################



            _generatePdf.SetConvertOptions(new WkHtmlToPdfOptions()
            {
                EnableForms = true,
                PageOrientation = Wkhtmltopdf.NetCore.Options.Orientation.Portrait,
                IsLowQuality = false,
                PageSize = Wkhtmltopdf.NetCore.Options.Size.A4,
                JavascriptDelay = 1000,
                MarginTop = 4,
                MarginBottom = 4,
                Encoding = "utf-8"
            });
            var pdf = _generatePdf.GetPDF(html);
            var ms = new MemoryStream(pdf);
            ms.Position = 0;
            //return pdf;
            return ms.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Fase x1: {ex.Message}: ({ex.ToString()})", ex);
            return new byte[] { };
        }
    }

    //var doc = new HtmlToPdfDocument()
    //{
    //    GlobalSettings =
    //              {
    //                ColorMode = ColorMode.Color,
    //                PaperSize = PaperKind.A4,
    //                Orientation = Orientation.Portrait,
    //                Margins = new MarginSettings(10, 4, 4, 4)
    //              },
    //    Objects =
    //              {
    //                new ObjectSettings()
    //                {
    //                    PagesCount = true,
    //                    Encoding = Encoding.UTF8,
    //                    UseExternalLinks = true,
    //                    HtmlContent = html,
    //                    WebSettings =
    //                    {
    //                        EnableIntelligentShrinking = false,
    //                        EnableJavascript = true,
    //                        Background = true,
    //                        enablePlugins = true,
    //                        MinimumFontSize = 1,
    //                        DefaultEncoding = "utf-8",
    //                        LoadImages = true,
    //                        PrintMediaType = true
    //                    },
    //                    LoadSettings = new LoadSettings
    //                    {
    //                        LoadErrorHandling = ContentErrorHandling.Skip,
    //                        JSDelay = 10000,
    //                        StopSlowScript = false,
    //                        ZoomFactor = 1.0d
    //                    },
    //                    ProduceForms = false,
    //                    UseLocalLinks = true,
    //                    IncludeInOutline = true
    //                }
    //              }
    //};
}