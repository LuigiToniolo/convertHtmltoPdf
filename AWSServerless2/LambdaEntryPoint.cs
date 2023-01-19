using Amazon.Lambda.AspNetCoreServer;

namespace AWSServerless2;

public class LambdaEntryPoint : Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
{
    protected override void Init(IWebHostBuilder builder)
    {
        RegisterResponseContentEncodingForContentType("multipart/form-data", ResponseContentEncoding.Base64);
        RegisterResponseContentEncodingForContentType("multipart/form-data", ResponseContentEncoding.Default);
        RegisterResponseContentEncodingForContentType("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ResponseContentEncoding.Default);
        RegisterResponseContentEncodingForContentType("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ResponseContentEncoding.Base64);
        RegisterResponseContentEncodingForContentType("application/octet-stream", ResponseContentEncoding.Default);
        RegisterResponseContentEncodingForContentType("application/octet-stream", ResponseContentEncoding.Base64);
        RegisterResponseContentEncodingForContentType("application/*", ResponseContentEncoding.Default);
        RegisterResponseContentEncodingForContentType("application/*", ResponseContentEncoding.Base64);
        builder.UseStartup<Startup>();
    }

    protected override void Init(IHostBuilder builder)
    {
    }
}