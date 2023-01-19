using Wkhtmltopdf.NetCore;
using Wkhtmltopdf.NetCore.Options;

namespace AWSServerless2
{
    public class WkHtmlToPdfOptions : ConvertOptions
    {
        [OptionFlag("--javascript-delay")]
        public int JavascriptDelay { get; set; }

        [OptionFlag("--margin-top")]
        public int MarginTop { get; set; }

        [OptionFlag("--margin-bottom")]
        public int MarginBottom { get; set; }

        [OptionFlag("--encoding")]
        public string Encoding { get; set; }

        [OptionFlag("--custom-header")]
        public string CustomHeader { get; set; } = " 'meta' 'charset=utf-8'";
    }
}
