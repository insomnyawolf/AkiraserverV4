using AkiraserverV4.Http.BaseContex;
using AkiraserverV4.Http.SerializeHelpers;
using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace SampleServer
{
    [Controller]
    public class SampleContext : BaseContext
    {
        public SampleService Service { get; set; }

        public SampleContext(SampleService service)
        {
            Service = service;
        }

        [Get("/[method]")]
        public string Count()
        {
            return $"Hello!\n{DateTime.Now}\nRequest: {Service.RequestNumber()}";
        }

        public class SampleClass
        {
            public string Text { get; set; }
            public DateTime CurrentTime { get; set; }
            public int RequestNumber { get; set; }
        }

        [Get("/[method]")]
        public JsonResult CountJson()
        {
            var response = new SampleClass()
            {
                Text = "Hello!",
                CurrentTime = DateTime.Now,
                RequestNumber = Service.RequestNumber()
            };
            return new JsonResult(response);
        }

        [Get("/[method]")]
        public void Void()
        {
            // Potato Method
        }

        [Get("/[method]")]
        public void NotAuth()
        {
            _ = Request.Path.Length > 0;
            Response.Status = AkiraserverV4.Http.BaseContex.Responses.HttpStatus.Unauthorized;
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
            await WriteDataAsync(test);
        }

        [Post("/[method]")]
        public async Task ReciveResend()
        {
            Response.AddContentLenghtHeader(Request.Body.Count);
            await WriteDataAsync(Request.Body);
        }

        //[NotFoundEndpoint]
        public string SampleFallback()
        {
            Response.Status = AkiraserverV4.Http.BaseContex.Responses.HttpStatus.NotFound;
            return "Overwritten";
        }
    }
}