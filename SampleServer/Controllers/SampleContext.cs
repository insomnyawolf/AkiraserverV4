using AkiraserverV4.Http.Context;
using AkiraserverV4.Http.SerializeHelpers;
using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace SampleServer
{
    [Controller]
    public class SampleContext : CustomBaseContext
    {
        public ISampleService Service { get; set; }

        public SampleContext(ISampleService service)
        {
            Service = service;
        }

        [Get("/[method]")]
        public Task<string> Count()
        {
            return Task.FromResult($"Hello!\n{DateTime.Now}\nRequest: {Service.RequestNumber()}");
        }

        [Get("/[method]")]
        public double Bench()
        {
            return Service.RequestPerSecond();
        }

        [Get("/[method]")]
        public void Restart()
        {
            Service.RequestRestart();
        }

        [Get("/Persona")]
        public string Tu(string nombre, string apellido)
        {
            return $"{nombre} {apellido}";
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
            _ = Request.HttpHeaders.Path.Length > 0;
            Response.HttpResponseHeaders.Status = AkiraserverV4.Http.Context.Responses.HttpStatus.Unauthorized;
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

        [Get("/[method]")]
        public async Task TestAsyncEmptyTask()
        {
            await Task.Delay(10);
        }

        [Get("/Time")]
        public string Time()
        {
            return $"Now: '{DateTime.Now}'";
        }

        [Get("/Request")]
        public string RequestMethod()
        {
            return JsonSerializer.Serialize(Request.HttpHeaders);
        }

//        [Get("/[method]")]
//        public async Task ServeLongFile()
//        {
//#warning need To Implement This Example
//            byte[] test = new byte[100000000];
//            Response.AddContentLenght(test.Length);
//            Response.WriteDataAsync(test);
//        }

        //[Post("/[method]")]
        //public async Task ReciveResend()
        //{
        //    Response.AddContentLenghtHeader(Request.Body.Count);
        //    await WriteDataAsync(Request.Body);
        //}

        //[NotFoundEndpoint]
        public string SampleFallback()
        {
            Response.HttpResponseHeaders.Status = AkiraserverV4.Http.Context.Responses.HttpStatus.NotFound;
            return "Overwritten";
        }
    }
}