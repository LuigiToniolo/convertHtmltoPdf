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
        this.HttpContext.Response.Headers.TryAdd("Content-Type", "application/json"); // --> tem que ter em todas
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

            if (html.Contains("<script"))
            {
                var scripts1 = html
                    .Split("<script", StringSplitOptions.None)
                    .SelectMany(line =>
                    {
                        var splitedLine = line.Split("</script>");

                        var isFirst = true;
                        var treated = splitedLine.Select(scriptLine =>
                        {
                            var isScript = line.Contains("</script>") && isFirst;
                            var script = (isScript ? "<script" : "") + scriptLine + (isScript ? "</script>" : "");
                            isFirst = false;

                            return script;
                        }).ToList();
                        return treated;
                    }).ToList();

                var htmlTreated = html;
                var unicodes = await GetResource<IEnumerable<UnicodeCharsItem>>("UnicodeChars");

                foreach (var originalPartScript in scripts1.Where(a => a.Contains("<script")))
                {
                    var scriptReplaced = originalPartScript;
                    foreach (var unicode in unicodes)
                    {
                        scriptReplaced = scriptReplaced.Replace(unicode.Char, unicode.Unicode);
                    }
                    htmlTreated = htmlTreated.Replace(originalPartScript, scriptReplaced);
                }

                html = htmlTreated;

            }


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
            return (byte[])pdf;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Fase x1: {ex.Message}: ({ex.ToString()})", ex);
            return new byte[] { };
        }
    }
}