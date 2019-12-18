using AkiraserverV4.Http.ContextFolder;
using AkiraserverV4.Http.ContextFolder.RequestFolder;
using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace SampleServer
{
    [Controller]
    public class SampleContext : Context
    {
        public SampleService Service { get; set; }

        public SampleContext(SampleService service)
        {
            Service = service;
        }

        [Get("/Count")]
        public string Counter()
        {
            return $"Hola!\nRequest: {Service.RequestNumber()}";
        }

        [Get("/[method]")]
        public void Void()
        {
            // Potato Method
        }

        [Get("/[method]")]
        public async Task<string> TestAsync()
        {
            HttpWebRequest request = WebRequest.CreateHttp("https://konachan.com/post.json?tags=nagishiro_mito");

            using WebResponse response = await request.GetResponseAsync();
            using Stream stream = response.GetResponseStream();
            using StreamReader reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        [Get("/Time")]
        public string Time()
        {
            return $"Now: '{DateTime.Now}'";
        }

        [Get("/Request")]
        public string RequestMethod()
        {
            return JsonSerializer.Serialize(Request.Headers);
        }

        [Get("/[method]")]
        public async Task ServeLongFile()
        {
#warning need To Implement This Example
            byte[] test = new byte[2321312];
            Response.AddContentLenghtHeader(test.Length);
            await WriteData(test);
        }

        [DefaultEndpoint]
        public string SampleFallback()
        {
            Response.Status = AkiraserverV4.Http.ContextFolder.ResponseFolder.HttpStatus.NotFound;
            return "404 NotFound";
        }
    }
}