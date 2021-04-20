* Enable MVC & Routing log/trace add  "Microsoft.AspNetCore.Mvc":"Trace","Microsoft.AspNetCore.Routing":"Trace" entries to appSettings.json
* To enable Kestral Connection logging
```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel((context, options) =>
                     {
                         //options.ConfigureEndpointDefaults(lo => lo.Protocols = HttpProtocols.Http2);
                         options.Listen(IPAddress.Loopback, 5000,lis => {
                             lis.UseConnectionLogging();
                         });
                     });

                    webBuilder.UseStartup<Startup>();
                });
```
*  To receive any kind of message in the POST body use Body Reader Directly.

```csharp
public async Task<ActionResult> ProcessOtlpMessage()
        {
            string result = string.Empty;
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                result = await reader.ReadToEndAsync();
            }
            Console.WriteLine($"ProcessOtlpMessage {result}");
            return new OkResult();
        }
```
or write input formatter
```csharp
 public void ConfigureServices(IServiceCollection services)
        {
services.AddControllers(mvcOptions =>
            {
                //mvcOptions.InputFormatters.Insert(0, new RawRequestBodyFormatter());
                //mvcOptions.InputFormatters.Insert(0, new RawJsonBodyInputFormatter());
                //mvcOptions.InputFormatters.Insert(1, new RawRequestBodyFormatter());
            })
 }   
            
public class RawJsonBodyInputFormatter : InputFormatter
    {
        public RawJsonBodyInputFormatter()
        {
            this.SupportedMediaTypes.Add("application/json");
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            using (var reader = new StreamReader(request.Body))
            {
                var content = await reader.ReadToEndAsync();
                return await InputFormatterResult.SuccessAsync(content);
            }
        }

        protected override bool CanReadType(Type type)
        {
            return type == typeof(string);
        }
    }
    /// <summary>
    /// Formatter that allows content of type text/plain and application/octet stream
    /// or no content type to be parsed to raw data. Allows for a single input parameter
    /// in the form of:
    /// 
    /// public string RawString([FromBody] string data)
    /// public byte[] RawData([FromBody] byte[] data)
    /// </summary>
    public class RawRequestBodyFormatter : InputFormatter
    {
        public RawRequestBodyFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/plain"));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/octet-stream"));
        }


        /// <summary>
        /// Allow text/plain, application/octet-stream and no content type to
        /// be processed
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Boolean CanRead(InputFormatterContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var contentType = context.HttpContext.Request.ContentType;
            if (string.IsNullOrEmpty(contentType) || contentType == "text/plain" ||
                contentType == "application/octet-stream")
                return true;

            return false;
        }

        /// <summary>
        /// Handle text/plain or no content type for string results
        /// Handle application/octet-stream for byte[] results
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            var contentType = context.HttpContext.Request.ContentType;


            if (string.IsNullOrEmpty(contentType) || contentType == "text/plain")
            {
                using (var reader = new StreamReader(request.Body))
                {
                    var content = await reader.ReadToEndAsync();
                    return await InputFormatterResult.SuccessAsync(content);
                }
            }
            if (contentType == "application/octet-stream")
            {
                using (var ms = new MemoryStream(2048))
                {
                    await request.Body.CopyToAsync(ms);
                    var content = ms.ToArray();
                    return await InputFormatterResult.SuccessAsync(content);
                }
            }

            return await InputFormatterResult.FailureAsync();
        }
    }
```
```csharp
Handling multiple routes in a method with Custome content type
[HttpPost]
[Route("routeA"]
[Route("routeB"]
[Route("routeC"]
public ActionResult SomeMethod()
{
  return new ContentResult() 
  {
     Content = "some data",
     ContentType = "content type relevent to the data"
  }
}
```
